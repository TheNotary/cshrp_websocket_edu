using Xunit;
using WebsocketEdu;
using System.IO;
using System.Text;
using Xunit.Abstractions;
using System.Threading;
using System;
using FluentAssertions;

namespace WebsocketEduTest
{
    public class UnitTest1
    {
        string validHttpUpgradeRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
        byte[] validWebsocketHello = new byte[] { 129, 133, 90, 120, 149, 83, 50, 29, 249, 63, 53 };
        byte[] validClientClose = new byte[] { 129, 133, 90, 120, 149, 83, 50, 29, 249, 63, 53 };
        string validHandshakeResponse = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";

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
            MockNetworkStreamProxy stream = new MockNetworkStreamProxy(CreateStreamWithTestStringFeedable(testHttpRequest));
            NetworkStreamReader sr = new NetworkStreamReader(stream);

            // When
            string websocketHeader = WebsocketExample.ReadHttpUpgradeRequestAndReturnWebsocketHeader(sr);

            // Then
            Assert.Equal(expectedWebsocketHeader, websocketHeader);
        }

        [Fact]
        public void ItRespondsCorrectlyToAValidHandshake()
        {
            // Given
            string expectedResponseWrites = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";
            string testHttpRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(testHttpRequest);

            // When
            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, 2);

            bool result = WebsocketExample.HandleHandshake(networkStreamProxy, headerBytes);

            // Then
            Assert.True(result);
            Assert.Equal(expectedResponseWrites, networkStreamProxy.GetWritesAsString());
        }

        [Fact]
        public void ItImmediatelyReturnsFalseIfTheStreamIsWebsocketData()
        {
            string data = "i don't start with the word GET";
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(CreateStreamWithTestStringFeedable(data));

            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, 2);

            bool result = WebsocketExample.HandleHandshake(networkStreamProxy, headerBytes);

            Assert.False(result);
        }

        [Fact]
        public void ItHandlesHandshakesMessagesAndClosesCorrectly()
        {
            //given
            MockNetworkStreamProxy networkStreamProxy = 
                new MockNetworkStreamProxy(new FeedableMemoryStream(validHttpUpgradeRequest));


            Thread handleClientMessageThread =
                new Thread(new ParameterizedThreadStart(PerformHandleClientMessage));

            // when
            //var t = new Thread(() => { Console.WriteLine(i); });
            //t.Start();
            handleClientMessageThread.Start(new object[] { networkStreamProxy, 2 });
            networkStreamProxy.PutBytes(validWebsocketHello);
            networkStreamProxy.PutBytes(validWebsocketHello);
            handleClientMessageThread.Join();

            // then
            Assert.Equal(validHandshakeResponse, networkStreamProxy.GetWritesAsString());
            Assert.Equal("71 69 129 133 90 120 149 83 50 29 249 63 53 ", networkStreamProxy.PrintBytesRecieved());
        }

        /* Parameters:
         *   - INetworkStream stream - Network stream to read from and write to throughout testing
         *   - int times - Number of times to call HandleClientMessage
         * */
        private void PerformHandleClientMessage(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            object[] array = (object[]) obj;
            INetworkStream stream = (INetworkStream)array[0];
            int times = (int)array[1];

            for(int i = 0; i < times; i++)
            {
                WebsocketExample.HandleClientMessage(stream);
            }
        }

        [Fact]
        public void ItWaitsForTheCompleteFrameToComeInEvenIfItsSentSlowDuringHandshaking()
        {
            // given
            string firstPartOfHandshake = validHttpUpgradeRequest.Substring(0, 3);
            string secondPartOfHandshake = validHttpUpgradeRequest.Substring(3);
            MockNetworkStreamProxy networkStreamProxy = 
                new MockNetworkStreamProxy(CreateStreamWithTestStringFeedable(firstPartOfHandshake));

            Thread handleHandshakeThread = 
                new Thread(new ParameterizedThreadStart(PerformHandleHandshakeInThread));

            // when
            handleHandshakeThread.Start(networkStreamProxy);

            // Then
            // this will block forever
            Thread.Sleep(100);
            networkStreamProxy.PutBytes(Encoding.UTF8.GetBytes(secondPartOfHandshake));
            handleHandshakeThread.Join();

            Assert.Equal(validHandshakeResponse, networkStreamProxy.GetWritesAsString());
        }

        private void PerformHandleHandshakeInThread(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            MockNetworkStreamProxy networkStreamProxy = (MockNetworkStreamProxy) obj;

            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, 2);

            WebsocketExample.HandleHandshake(networkStreamProxy, headerBytes);
        }

        private FeedableMemoryStream CreateStreamWithTestStringFeedable(string testString)
        {
            return new FeedableMemoryStream(testString);
        }
    }
}                                                            