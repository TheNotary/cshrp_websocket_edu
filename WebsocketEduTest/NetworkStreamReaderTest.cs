using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebsocketEdu;
using Xunit;

namespace WebsocketEduTest
{
    public class NetworkStreamReaderTest
    {

        [Fact]
        public void ItCanReadALineFromTheStreamAtATime()
        {
            // Given
            string firstLine = "GET / HTTP/1.1";
            string testHttpRequest = $"{firstLine}\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(CreateStreamWithTestStringFeedable(testHttpRequest));
            NetworkStreamReader nsr = new NetworkStreamReader((Stream) networkStreamProxy);

            // When
            string myText = nsr.ReadUntilCarriageReturn();

            // Then
            Assert.Equal(firstLine, myText);
        }

        [Fact]
        public void ItBlocksWhileReadingALineUntilTheStreamHasACarriageReturn()
        {
            // Given
            string firstLine = "GET / HTTP/1.1";
            byte[] eolBytes = Encoding.UTF8.GetBytes("\r\n");

            MockNetworkStreamProxy networkStreamProxy = 
                new MockNetworkStreamProxy(CreateStreamWithTestStringFeedable(firstLine));

            // When
            Thread t = new Thread(new ParameterizedThreadStart(ReadNetworkStreamInThreadAndEchoToWriteStream));
            t.Start(networkStreamProxy);

            // Then
            Thread.Sleep(50);
            networkStreamProxy.PutBytes(eolBytes);
            t.Join();
            Assert.Equal(firstLine, networkStreamProxy.GetWritesAsString());
        }

        private void ReadNetworkStreamInThreadAndEchoToWriteStream(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            MockNetworkStreamProxy networkStreamProxy = (MockNetworkStreamProxy)obj;
            NetworkStreamReader nsr = new NetworkStreamReader((Stream) networkStreamProxy);

            string line = nsr.ReadUntilCarriageReturn();

            byte[] lineBytes = Encoding.UTF8.GetBytes(line);

            networkStreamProxy.Write(lineBytes, 0, lineBytes.Length);
        }

        private FeedableMemoryStream CreateStreamWithTestStringFeedable(string testString)
        {
            FeedableMemoryStream stream = new FeedableMemoryStream();

            byte[] buffer = Encoding.ASCII.GetBytes(testString);
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
