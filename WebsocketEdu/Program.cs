using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace WebsocketEdu
{
    /// <summary>
    /// This application is a basic example of a working websocket server implementation.  
    /// </summary>
    public class WebsocketExample
    {
        private static int threadPoolSize = 1;
        private static LinkedList<Thread> threads = new LinkedList<Thread>();
        private static int port = 80;
        private static TcpListener? server;

        static void Main(string[] args)
        {
            server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:{0}.{1}Waiting for a connection...", port, Environment.NewLine);

            ThreadManagementLoop();

            Console.WriteLine("Press 'q' to quit at any time.");
            while (Console.Read() != 'q') ;
        }

        public static void ThreadManagementLoop()
        {
            while (true)
            {
                if (threads.Count < threadPoolSize)
                {
                    //GC.Collect(); // For testing memory leaks
                    Thread t = new Thread(new ParameterizedThreadStart(HandleNewClientConnectionInThread));
                    t.Start(server);
                    threads.AddLast(t);
                    Console.WriteLine("Started new thread, threadcount at " + threads.Count);
                }
                // FIXME: that weird bug where the console doesn't update is probably because I'm using this while:true loop and not explicitly managing updates
                Thread.Sleep(500);

                // Clean up dead threads
                if (threads.First == null) continue;
                LinkedListNode<Thread> node = threads.First;
                for (int i = 0; i < threads.Count; i++)
                {
                    Thread t = node.Value;
                    if (!t.IsAlive)
                        threads.Remove(t);
                    if (node.Next == null) break;
                    node = node.Next;
                }
            }
        }

        public static void HandleNewClientConnectionInThread(object? server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            TcpClient tcpClient = ((TcpListener) server).AcceptTcpClient();
            string remoteIp = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("A client connected from {0}", remoteIp);
            INetworkStream networkStream = new NetworkStreamProxy( tcpClient.GetStream() );

            while (!tcpClient.Connected) ;
            while (tcpClient.Connected)
            {
                while (!networkStream.DataAvailable) ; // block here till we have data

                // wait for the first 2 bytes to be available.  Websocket messages consist of a two byte header detailing 
                // the shape of the incoming websocket frame...
                while (tcpClient.Available < 2) ;

                Console.WriteLine("New Bytes ready for processing from client: " + tcpClient.Available);

                try
                {
                    HandleClientMessage(networkStream);
                }
                catch (ClientClosedConnectionException ex)
                {
                    Console.WriteLine("Bytes in Frame were:\r\n" + networkStream.PrintBytesRecieved());
                    Console.WriteLine("  << Client Sent Close, dropping Stream >>\r\n" + ex.Message);
                    networkStream.SourceStream.Close();
                    tcpClient.Close();
                    tcpClient.Dispose();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Bytes in Frame were:\r\n" + networkStream.PrintBytesRecieved());
                    Console.WriteLine("  << Exception encountered, closing client :p >>\r\n" + ex.Message);
                    networkStream.SourceStream.Close();
                    tcpClient.Close();
                    tcpClient.Dispose();
                    break;
                }
                Console.WriteLine("Bytes in Frame were:\r\n" + networkStream.PrintBytesRecieved());

                networkStream.ClearDebugBuffer();
            }
        }

        public static void HandleClientMessage(INetworkStream networkStream)
        {
            // Get the client's data now that they've at least gotten to the "GE" part of the HTTP upgrade request or the frame header.
            Byte[] headerBytes = new Byte[2];
            networkStream.Read(headerBytes, 0, headerBytes.Length);

            if (HandleHandshake(networkStream, headerBytes)) return;

            // Handle ordinary websocket communication
            HandleWebsocketMessage(networkStream, headerBytes);
        }

        static public void HandleWebsocketMessage(INetworkStream stream, Byte[] headerBytes)
        {
            var websocketReader = new WebsocketReader(stream, headerBytes);
            WebsocketFrame websocketFrame = websocketReader.ConsumeFrameFromStream();

            if (!websocketFrame.isMasked)
            {
                throw new NotSupportedException("mask bit not set.  Masks MUST be set by the client when sending messages to prevent cache poisoning attacks leveraged against internet infrastructure like proxies and cyber warfar appliances.");
            }

            switch (websocketFrame.opcode)
            {
                case (0x01):  // text message
                    Console.WriteLine("> Client: {0}", Encoding.UTF8.GetString(websocketFrame.decodedPayload));
                    break;
                case (0x08):  // close message
                    byte[] response = BuildCloseFrame(websocketFrame.decodedPayload);
                    stream.Write(response, 0, response.Length);
                    string closeCodeString = websocketFrame.closeCode != 0
                        ? "Close frame code was " + websocketFrame.closeCode
                        : "There was no close code.";
                    throw new ClientClosedConnectionException("The client sent a close frame.  " + closeCodeString);
            }
        }

        public static byte[] BuildCloseFrame(byte[] closeCodeBytes)
        {
            MemoryStream output = new MemoryStream();
            //byte[] closeCodeBytes = BitConverter.GetBytes(closeCode);
            output.WriteByte(0b10001000); // opcode for a finished, closed frame
            output.WriteByte(0x02);       // length of close payload being 2, this message isn't masked, they say there's no vulnerability to the server...
            output.Write(closeCodeBytes, 0, closeCodeBytes.Length);
            return output.ToArray();
        }





        public static bool HandleHandshake(INetworkStream stream, byte[] headerBytes)
        {
            String data = Encoding.UTF8.GetString(headerBytes);

            if (data != "GE")  // The handshake always begins with the line "GET " and websocket frames can't begin with G unless an extension was negotiated
                return false;


            //HttpHandshaker handshaker = new HttpHandshaker(stream, headerBytes);

            NetworkStreamReader sr = new NetworkStreamReader(stream);
            string inboundWebSocketHeaderLine = ReadHttpUpgradeRequestAndReturnWebsocketHeader(sr);
            string webSocketKey = GenerateResponseWebsocketHeaderValue(inboundWebSocketHeaderLine);
            RespondToHandshake(stream, webSocketKey);
            Console.WriteLine("Upgraded client to websockets.");
            return true;
        }

        static void RespondToHandshake(INetworkStream stream, string webSocketHeader)
        {
            const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                + "Connection: Upgrade" + eol
                + "Upgrade: websocket" + eol
                + "Sec-WebSocket-Accept: " + webSocketHeader + eol
                + eol);
            stream.Write(response, 0, response.Length);
            Console.WriteLine("Response to handshake written to stream");
        }

        // This function Read stream until you get to /r/n/r/n meaning the end of their opening HTTP upgrade request
        public static string ReadHttpUpgradeRequestAndReturnWebsocketHeader(NetworkStreamReader sr)
        {
            string webSocketHeader = "";
            string requestedResource = "";
            string websocketKey = "Sec-WebSocket-Key:";
            string resourceRequestedLine = "GET /";
            string debug = "GE";
            string line;
            bool processedFirstLine = false;
            while (true)  // TODO: implement a receive timeout
            {
                line = sr.ReadUntilCarriageReturn();
                debug += line + "\r\n";
                if (line == "") break;  // EOF reached

                if (!processedFirstLine)  // ProcessFirstLine()
                {
                    string firstLine = "GE" + line;
                    if (firstLine.Length >= 5 &&
                        firstLine.Substring(0, 5) == resourceRequestedLine)
                    {
                        line = "GE" + line;

                        requestedResource = line
                            .Replace("GET /", "/")
                            .Replace(" HTTP/1.1", "");
                    }
                    processedFirstLine = true;
                }
                
                // handle extracting websocket key
                if (line.StartsWith(websocketKey))
                {
                    webSocketHeader = line.Substring(websocketKey.Length).Trim();
                }

                // check if we've got a double /r/n
                if (line == "") // if we're out of data and we received an empty line
                    break;
            }

            var debugMessage = "Requested Resource: " + requestedResource + "\r\n"
                             + "Websocket Header: " + webSocketHeader + "\r\n"
                             + "Handshake Request: \r\n" + debug;
            Console.WriteLine(debugMessage);

            // ValidateThatThisIsReallyAValidWebsocketUpgradeRequest()
            if (webSocketHeader == "")
                throw new Exception("could not extract websocket header from handshake.  Wrong number?");

            return webSocketHeader;
        }

        static string GenerateResponseWebsocketHeaderValue(String inboundWebSocketKey)
        {
            return Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(inboundWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        }

    }

}
