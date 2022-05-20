using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace WebsocketEdu
{
    public class TcpController
    {
        public static void HandleNewClientConnectionInThread(object? server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            TcpClient tcpClient = ((TcpListener)server).AcceptTcpClient();
            string remoteIp = GetRemoteIp(tcpClient);

            Console.WriteLine("A client connected from {0}", remoteIp);
            INetworkStream networkStream = new NetworkStreamProxy(tcpClient.GetStream());

            while (!tcpClient.Connected) ;
            while (tcpClient.Connected)
            {
                while (!networkStream.DataAvailable) ; // block here till we have data

                // wait for the first 2 bytes to be available.  Websocket messages consist of a two byte header detailing 
                // the shape of the incoming websocket frame...
                while (tcpClient.Available < 2) ;

                Console.WriteLine("New Bytes ready for processing from client: " + tcpClient.Available);
                string msg;

                try
                {
                    msg = HandleClientMessage(networkStream);
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

        public static string HandleClientMessage(INetworkStream networkStream)
        {
            // Get the client's data now that they've at least gotten to the "GE" part of the HTTP upgrade request or the frame header.
            Byte[] headerBytes = new Byte[2];
            networkStream.Read(headerBytes, 0, headerBytes.Length);

            if (HandleHandshake(networkStream, headerBytes)) return "";

            // Handle ordinary websocket communication
            return HandleWebsocketMessage(networkStream, headerBytes);
        }

        static public string HandleWebsocketMessage(INetworkStream stream, Byte[] headerBytes)
        {
            WebsocketClient websocketClient = new WebsocketClient(stream, headerBytes);
            WebsocketFrame websocketFrame = websocketClient.ConsumeFrameFromStream();

            if (!websocketFrame.isMasked)
                throw new NotSupportedException("mask bit not set.  Masks MUST be set by the client when sending messages to prevent cache poisoning attacks leveraged against internet infrastructure like proxies and cyber warfar appliances.");

            switch (websocketFrame.opcode)
            {
                case (0x01):  // text message
                    string msg = Encoding.UTF8.GetString(websocketFrame.decodedPayload);
                    Console.WriteLine("> Client: {0}", msg);
                    new CommandRouter(websocketClient).HandleCommand(msg);
                    return msg;
                case (0x08):  // close message
                    byte[] response = BuildCloseFrame(websocketFrame.decodedPayload);
                    stream.Write(response, 0, response.Length);
                    string closeCodeString = websocketFrame.closeCode != 0
                        ? "Close frame code was " + websocketFrame.closeCode
                        : "There was no close code.";
                    throw new ClientClosedConnectionException("The client sent a close frame.  " + closeCodeString);
                default:
                    Console.WriteLine("Unknown websocket Opcode received from client: {0}", websocketFrame.opcode);
                    return "";
                    throw new NotSupportedException();
            }
        }


        public static bool HandleHandshake(INetworkStream stream, byte[] headerBytes)
        {
            String data = Encoding.UTF8.GetString(headerBytes);

            if (data != "GE")  // The handshake always begins with the line "GET " and websocket frames can't begin with G unless an extension was negotiated
                return false;

            HttpHandshaker handshaker = new HttpHandshaker(stream, headerBytes);
            handshaker.ConsumeHttpUpgradeRequestAndCollectWebsocketHeader();
            handshaker.RespondToHandshake();
            Console.WriteLine("Upgraded client to websockets.");
            return true;
        }

        public static byte[] BuildCloseFrame(byte[] closeCodeBytes)
        {
            MemoryStream output = new MemoryStream();
            output.WriteByte(0b10001000); // opcode for a finished, closed frame
            output.WriteByte(0x02);       // length of close payload being 2, this message isn't masked, they say there's no vulnerability to the server...
            output.Write(closeCodeBytes, 0, closeCodeBytes.Length);
            return output.ToArray();
        }

        private static string GetRemoteIp(TcpClient tcpClient)
        {
            if (tcpClient == null || tcpClient.Client == null || tcpClient.Client.RemoteEndPoint == null || tcpClient.Client.RemoteEndPoint.ToString() == null)
                return "NONE";
            string? omg = tcpClient.Client.RemoteEndPoint.ToString();
            if (omg == null)
                return "NONE";
            return omg;
        }

    }
}
