using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace WebsocketEdu
{

    /*
     * This application is a basic example of a working websocket server implementation.  
     * 
     * */
    public class WebsocketExample
    {
        private static int threadPoolSize = 1;
        private static LinkedList<Thread> threads = new LinkedList<Thread>();

        static void Main(string[] args)
        {
            // Console.WriteLine(new string('*', 65535) + "hello");
            TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 80);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);

            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                if (threads.Count < threadPoolSize)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(HandleNewClientConnection));
                    t.Start(server);
                    threads.AddLast(t);
                }
            }
        }

        public static void HandleNewClientConnection(object? server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            // This could be it's own thread while thread pool !full
            TcpClient tcpClient = ((TcpListener) server).AcceptTcpClient();
            Console.WriteLine("A client connected.\n");
            NetworkStream networkStream = tcpClient.GetStream();



            while (!tcpClient.Connected) ;
            while (tcpClient.Connected)
            {
                while (!networkStream.DataAvailable) ; // block here till we have data

                HandleClientCommunication(tcpClient, networkStream);
            }
            tcpClient.Close();
        }


        static void HandleClientCommunication(TcpClient tcpClient, NetworkStream networkStream)
        {
            // 1.
            // wait for the first 3 bytes to be available.  Websocket messages consist of a three byte header detailing 
            // the shape of the incoming websocket frame...
            while (tcpClient.Available < 3) ;

            // 2. 
            // Get the client's data now that they've at least gotten to the "GET" part or the frame header
            Byte[] bytes = new Byte[3];
            networkStream.Read(bytes, 0, bytes.Length);
            String data = Encoding.UTF8.GetString(bytes);

            Console.WriteLine("New Bytes ready for processing from client: " + tcpClient.Available);

            if (HandleHandshake(networkStream, data)) return;

            // Handle ordinary communication
            HandleMessage(networkStream, bytes, tcpClient);
        }

        static void HandleMessage(NetworkStream stream, Byte[] bytes, TcpClient client)
        {
            bool fin  = (bytes[0] & 0b10000000) != 0,
                 mask = (bytes[1] & 0b10000000) != 0;  // must be true, "All messages from the client to the server have this bit set"
            int opcode = bytes[0] & 0b00001111;        // expecting 0x0001, indicating a text message

            // determine the message length
            object[] result = determineMessageLength(bytes);
            ulong msglen = (ulong)result[0];
            int offset = (int)result[1];


            // TODO: Check if we have all of the message buffered yet
            if ((ulong)bytes.Length < msglen - (ulong)offset)        // we got 65495 when we sent the payload, but we need 65538
            {
                ulong bytesNeeded = msglen - (ulong)offset - (ulong)bytes.Length;

                Console.WriteLine("We don't have enough of the message!");
            }


            if (msglen == 0)
                Console.WriteLine("msglen == 0");
            else if (mask)
            {
                Byte[] decoded = decodeMessage(bytes, offset);    // only has up to 65521 properly decoded, the rest are empty bytes...

                string text = Encoding.UTF8.GetString(decoded);
                Console.WriteLine("Client: {0}", text);
            }
            else {
                Console.WriteLine("mask bit not set");
                string text = Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
                Console.WriteLine("Client: {0}", text);
            }

        }

        static object[] determineMessageLength(Byte[] bytes)
        {
            int msglen = bytes[1] & 0b01111111;
            int offset = 2;
            ulong msglen64 = (ulong)msglen;

            if (msglen == 126)// 126 signifies an extended payload size of 16bits
            {
                // We could use the ToUInt16, but it's less supported across frameworks than just using longs which are pretty big
                msglen64 = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] });
                offset = 4;
            }
            if (msglen == 127)
            {
                msglen64 = BitConverter.ToUInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] });
                offset = 10;

                // To test the below notes, we need to manually buffer larger messages since the NIC's autobuffering is too
                // latency friendly for this code to run ordinarily
                Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
                // i don't really know the byte order, please edit this
                // msglen = BitConverter.ToUInt64(new byte[] { data[5], data[4], data[3], data[2], data[9], data[8], data[7], data[6] }, 0);
                // offset = 10;
            }

            return new object[] { msglen64, offset };
        }


        static byte[] decodeMessage(Byte[] bytes, int offset)
        {
            Byte[] decoded = new Byte[bytes.Length];  // TODO, shouldn't this be bytes - offset actually?

            // read mask, has an offset of 2 or 4 at this point depending on... something to do with ... chance?
            byte[] mask = new byte[4] { bytes[offset],
                                    bytes[offset + 1],
                                    bytes[offset + 2],
                                    bytes[offset + 3] };


            offset += 4;
            for (int i = 0; i < bytes.Length - offset; i++)
            {
                decoded[i] = (Byte)(bytes[offset + i] ^ mask[i % 4]);
            }
            return decoded;
        }

        public static bool HandleHandshake(Stream stream, string data)
        {
            if (!Regex.IsMatch(data, "^GET"))
                return false;

            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            string webSocketHeader = ReadHttpUpgradeRequestAndReturnWebsocketHeader(sr);
            string webSocketKey = GenerateResponseWebsocketHeaderValue(webSocketHeader);
            RespondToHandshake(stream, webSocketKey);
            return true;
        }


        static void RespondToHandshake(Stream stream, string webSocketHeader)
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
        public static string ReadHttpUpgradeRequestAndReturnWebsocketHeader(StreamReader sr)
        {
            string webSocketHeader = "";
            string websocketKey = "Sec-WebSocket-Key:";
            string debug = "";
            string line = "";
            string priorLine = "";
            while (true)
            {
                priorLine = line;
                line = sr.ReadLine();
                debug += line + "\r\n";
                if (line == null) break;  // EOF reached

                // handle extracting websocket key
                if (line.StartsWith(websocketKey))
                {
                    webSocketHeader = line.Substring(websocketKey.Length).Trim();
                }

                // check if we've got a double /r/n
                if (line == "" && priorLine == "")
                    break;
            }
            return webSocketHeader;
        }


        static string GenerateResponseWebsocketHeaderValue(String data)
        {
            string webSocketKey = new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim();
            return Convert.ToBase64String(
                    SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))
                );
        }

    }

}
