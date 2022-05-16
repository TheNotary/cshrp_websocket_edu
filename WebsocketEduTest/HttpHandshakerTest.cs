using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;
using Xunit;
using Xunit.Abstractions;

namespace WebsocketEduTest
{
    public class HttpHandshakerTest : BaseTest
    {
        public HttpHandshakerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ItReadsHttpUpgradeRequestsAndGetsTheWebsocketHeader()
        {
            // Given
            string expectedInboundWebsocketKey = "fakewebsocketkey";
            string expectedRequestedResource = "/blah";
            string expectedResponseWebsocketHeaderValue = "tL4Z2RDPyz6g5sFlM5Fif+rvXTI=";
            byte[] headerBytes = Encoding.UTF8.GetBytes("GE");
            string testHttpRequest = $"T {expectedRequestedResource} HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: {expectedInboundWebsocketKey}\r\n\r\n";
            MockNetworkStreamProxy stream = new MockNetworkStreamProxy(testHttpRequest);
            HttpHandshaker handshaker = new HttpHandshaker(stream, headerBytes);

            // When
            handshaker.ConsumeHttpUpgradeRequestAndCollectWebsocketHeader();

            // Then
            Assert.Equal(expectedInboundWebsocketKey, handshaker.inboundWebSocketKey);
            Assert.Equal(expectedRequestedResource, handshaker.requestedResource);
            Assert.Equal(expectedResponseWebsocketHeaderValue, handshaker.responseWebsocketHeaderValue);
        }

    }
}
