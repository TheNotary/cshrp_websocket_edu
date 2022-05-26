using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;
using Xunit.Abstractions;

namespace WebsocketEduTest
{
    public class BaseTest
    {
        string validHttpUpgradeRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: zzz\r\n\r\n";
        byte[] validWebsocketHello = new byte[] { 129, 133, 90, 120, 149, 83, 50, 29, 249, 63, 53 };
        byte[] validClientClose = new byte[] { 136, 130, 104, 40, 78, 91, 107, 193 };
        string validHandshakeResponse = "HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: EJ5xejuUCHQkIKE2QxDTDCDws8Q=\r\n\r\n";

        public WebsocketClient CreateWebsocketClient(byte[] streamBytes)
        {
            MockNetworkStreamProxy networkStreamProxy = new MockNetworkStreamProxy(streamBytes);
            byte[] headerBytes = new byte[2];
            networkStreamProxy.Read(headerBytes, 0, headerBytes.Length);
            WebsocketClient websocketClient = new WebsocketClient(networkStreamProxy, headerBytes);
            return websocketClient;
        }

        public WebsocketClient CreateWebsocketClient()
        {
            byte[] headerBytes = Encoding.UTF8.GetBytes("GE");
            MockNetworkStreamProxy nsp = new MockNetworkStreamProxy("T /blah");
            WebsocketClient websocketClient = new WebsocketClient(nsp, headerBytes);
            return websocketClient;
        }
    }
}
