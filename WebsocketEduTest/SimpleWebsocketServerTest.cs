﻿using FluentAssertions;
using System.Threading;
using WebsocketEdu;
using Xunit;

namespace WebsocketEduTest
{
    public class SimpleWebsocketServerTest : BaseTest
    {
        [Fact]
        public void ItCanSerializeWebsocketsIntoBytes()
        {
            // when
            string actualPassword = SimpleWebsocketServer.GenerateRandomPassword();

            // then
            actualPassword.Length.Should().Be(10);
        }

        [Fact]
        public void ItCanBeBootedAndClientsCanConnectByIpAndPort()
        {
            string listenAddress = "127.0.0.1";
            int listenPort = 80;

            // given
            SimpleWebsocketServer simpleWebsocketServer = new SimpleWebsocketServer(listenAddress, listenPort);
            var t = new Thread(() =>
            {
                simpleWebsocketServer.Start();
            }); t.Start();

            // when
            SimpleWebsocketClient client = new SimpleWebsocketClient(listenAddress, listenPort);
            client.Handshake();

            client.SendMessage("hello");
            client.SendMessage("/auth " + simpleWebsocketServer.adminPassword);
            client.SendMessage("/close");

            // then
            bool threadJoined = t.Join(600);
            threadJoined.Should().BeTrue();
        }

    }
}
