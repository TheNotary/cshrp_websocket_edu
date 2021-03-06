using Xunit;
using System.Text;
using Xunit.Abstractions;
using System.Threading;
using System;
using FluentAssertions;
using WebsocketEdu;
using WebsocketEdu.Extensions;

namespace WebsocketEduTest
{
    public class TcpControllerTest : BaseTest
    {
        string validHttpUpgradeRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
        byte[] validWebsocketHello = new byte[] { 129, 133, 90, 120, 149, 83, 50, 29, 249, 63, 53 };
        byte[] validClientClose = new byte[] { 136, 130, 104, 40, 78, 91, 107, 193 };
        string validHandshakeResponse = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";

        [Fact]
        public void ItRespondsCorrectlyToAValidHandshake()
        {
            // Given
            string expectedResponseWrites = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";
            string testHttpRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(testHttpRequest);
            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, 2);

            // When
            bool result = TcpController.HandleHandshake(networkStreamProxy, headerBytes);

            // Then
            Assert.True(result);
            Assert.Equal(expectedResponseWrites, networkStreamProxy.GetWritesAsString());
        }

        [Fact]
        public void ItImmediatelyReturnsFalseIfTheStreamIsWebsocketData()
        {
            string data = "i don't start with the word GET";
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(data);

            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, 2);

            bool result = TcpController.HandleHandshake(networkStreamProxy, headerBytes);

            Assert.False(result);
        }

        [Fact]
        public void ItHandlesHandshakesMessagesAndClosesCorrectly()
        {
            // given
            WebsocketClient websocketClient = CreateWebsocketClient(Encoding.UTF8.GetBytes(validHttpUpgradeRequest));
            MockNetworkStreamProxy networkStreamProxy = (MockNetworkStreamProxy)websocketClient.Stream;
            ChannelBridge c = new ChannelBridge("");

            // when
            var t = new Thread(() => {
                TcpController.HandleClientMessage(websocketClient, c);
                TcpController.HandleClientMessage(websocketClient, c);
                Assert.Throws<ClientClosedConnectionException>(() => TcpController.HandleClientMessage(websocketClient, c));
            }); t.Start();
            networkStreamProxy.PutBytes(validWebsocketHello);
            networkStreamProxy.PutBytes(validClientClose);
            t.Join();

            // then
            Assert.Equal(validHandshakeResponse, networkStreamProxy.GetWritesAsString().Substring(0, validHandshakeResponse.Length));
            validWebsocketHello.Should().BeSubsetOf(networkStreamProxy.GetBytesRecieved());
            validClientClose.Should().BeSubsetOf(networkStreamProxy.GetBytesRecieved());
        }

        [Fact]
        public void ItWaitsForTheCompleteFrameToComeInEvenIfItsSentSlowDuringHandshaking()
        {
            // given
            string firstPartOfHandshake = validHttpUpgradeRequest.Substring(0, 3);
            string secondPartOfHandshake = validHttpUpgradeRequest.Substring(3);
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(firstPartOfHandshake);

            Thread handleHandshakeThread = 
                new Thread(new ParameterizedThreadStart(PerformHandleHandshakeInThread));

            // when
            handleHandshakeThread.Start(networkStreamProxy);

            // then
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

            TcpController.HandleHandshake(networkStreamProxy, headerBytes);
        }

    }
}                                                            