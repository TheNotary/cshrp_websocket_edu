using FluentAssertions;
using Xunit;
using WebsocketEduTest.Extensions;
using WebsocketEdu;

namespace WebsocketEduTest
{
    public class WebsocketReaderTest
    {
        string validHttpUpgradeRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
        byte[] validWebsocketHello = new byte[] { 129, 133, 90, 120, 149, 83, 50, 29, 249, 63, 53 };
        byte[] validClientClose = new byte[] { 136, 130, 104, 40, 78, 91, 107, 193 };
        string validHandshakeResponse = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";

        [Fact]
        public void ItCanConsumeAHelloWebsocketMessageAndOutputAFrame()
        {
            // given
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(validWebsocketHello);
            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, headerBytes.Length);
            WebsocketClient websocketReader = new WebsocketClient(networkStreamProxy, headerBytes);

            // when
            WebsocketFrame frame = websocketReader.ConsumeFrameFromStream();

            // then
            frame.opcode.Should().Be(0x01);
            frame.fin.Should().BeTrue();
            frame.isMasked.Should().BeTrue();
            frame.mask.Should().Equal(new byte[4] { 90, 120, 149, 83 });
            frame.payloadLength.Should().Be(5);
            frame.decodedPayload.Should().Equal("hello".ToBytes());
        }

        [Fact]
        public void ItCanRecognizeCloseFramesCorrectly()
        {
            // given
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(validClientClose);
            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, headerBytes.Length);
            WebsocketClient websocketReader = new WebsocketClient(networkStreamProxy, headerBytes);

            // when
            WebsocketFrame frame = websocketReader.ConsumeFrameFromStream();

            // then
            frame.closeCode.Should().Be(1001);
            frame.closeCodeReason.Should().Be("");
        }
    }
}
