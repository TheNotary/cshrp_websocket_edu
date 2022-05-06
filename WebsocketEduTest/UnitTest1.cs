using Xunit;
using WebsocketEdu;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using Xunit.Abstractions;
using System.IO.Pipes;
using Microsoft.Win32.SafeHandles;

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
        public void ItRespondsCorrectlyToAValidHandshake()
        {
            string testHttpRequest = $"GET / HTTP/1.1\r\nHost: server.example.com\r\nUpgrade: websocket\r\nSec-WebSocket-Key: {expectedWebsocketHeader}\r\n\r\n";
            Stream stream = CreateStreamWithTestString(testHttpRequest);
            byte[] headerBytes = new byte[2];
            stream.Read(headerBytes, 0, 2);

            var mockedSocket = new MockSocketProxy();

            bool result = WebsocketExample.HandleHandshake(stream, headerBytes);


        }

        [Fact]
        public void ItImmediatelyReturnsFalseIfTheStreamIsWebsocketData()
        {
            string data = "i don't start with the word GET";
            Stream stream = CreateStreamWithTestString(data);
            byte[] headerBytes = new byte[2];
            stream.Read(headerBytes, 0, 2);

            bool result = WebsocketExample.HandleHandshake(stream, headerBytes);

            Assert.False(result);
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