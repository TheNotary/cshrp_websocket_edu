using Xunit;
using WebsocketEdu;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using Xunit.Abstractions;

namespace WebsocketEduTest
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ItReadsHttpUpgradeRequestsAndGetsTheWebsocketHeader()
        {
            // Given
            string expectedWebsocketHeader = "websocketblah";
            string testHttpRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: {expectedWebsocketHeader}\r\n\r\n";
            Stream stream = CreateStreamWithTestString(testHttpRequest);
            StreamReader sr = new StreamReader(stream);

            // When
            string websocketHeader = WebsocketExample.ReadHttpUpgradeRequestAndReturnWebsocketHeader(sr);

            // Then
            Assert.Equal(expectedWebsocketHeader, websocketHeader);
        }

        [Fact]
        public void ItRespondsCorrectlyToAWHandshake()
        {

        }

        [Fact]
        public void ItImmediatelyReturnsFalseIfTheStreamIsWebsocketData()
        {
            string data = "i don't start with the word GET";
            Stream stream = CreateStreamWithTestString(data);

            bool result = WebsocketExample.HandleHandshake(stream, data);

            Assert.False(result);
        }

        [Fact]
        public void ItCanUseTcpStreamsToHandleThisDuplexIssue()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8881);
            server.Start();

            Thread t = new Thread(new ParameterizedThreadStart(ListenToClientAndLogData));
            t.Start(server);

            TcpClient client = new TcpClient("127.0.0.1", 8881);

            Socket socket = client.Client;
            socket.Send(new byte[] { 0b00000001 });
            socket.Send(new byte[] { 0b00000010 });
            socket.Send(new byte[] { 0b00000011 });
            socket.Send(new byte[] { 0b11111111 }); // this byte tells the client to close down
            client.Client.Close();

            t.Join();

            Assert.True(true);
        }

        private void ListenToClientAndLogData(object? svr)
        {
            if (svr == null) throw new ArgumentNullException(nameof(svr));
            TcpListener server = (TcpListener) svr;
            TcpClient client = server.AcceptTcpClient();

            Socket socket = client.Client;
            NetworkStream stream = client.GetStream();

            while (!client.Connected) ;
            while (client.Connected)
            {
                while (!stream.DataAvailable) ; // block here till we have data
                int myByte = stream.ReadByte();
                output.WriteLine(((int) myByte).ToString());
                if (myByte == 255) client.Close(); // 6 is our magic disconnect byte
            }
        }

        [Fact]
        public void ItCanUseFeedableMemoryStreams()
        {
            FeedableMemoryStream fms = new FeedableMemoryStream();

            Thread t = new Thread(new ParameterizedThreadStart(ListenToFeedableMemoryStream));
            t.Start(fms);


            fms.PutByte(1);
            fms.PutByte(2);
            fms.PutByte(3);
            fms.PutByte(255);

            t.Join();
        }

        private void ListenToFeedableMemoryStream(object? strm)
        {
            if (strm == null) throw new ArgumentNullException(nameof(strm));
            Stream stream = (Stream)strm;

            while (true)
            {
                if (stream.Position < stream.Length)
                {
                    int myByte = stream.ReadByte();
                    if (myByte == 255) break; // A 255 byte can signal the end of the stream
                    output.WriteLine("Data Recieved: " + myByte);
                }
            }
        }

        private Stream CreateStreamWithTestString(string testString)
        {
            Stream stream = new MemoryStream();

            byte[] buffer = Encoding.ASCII.GetBytes(testString);
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}