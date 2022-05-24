using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;
using WebsocketEdu.Extensions;
using Xunit;

namespace WebsocketEduTest
{
    public class CommandRouterTest : BaseTest
    {

        [Fact]
        public void ItCanHandleAuthenticationCommands()
        {
            // given
            IDictionary<string, string> hash = new Dictionary<string, string>();
            hash.Add("adminPassword", "FAKEPASS");
            IConfigurationRoot conf = new ConfigurationBuilder().AddInMemoryCollection(hash).Build();
            WebsocketClient websocketClient = CreateWebsocketClient();
            CommandRouter commandRouter = new CommandRouter(websocketClient, conf);
            string command = "/auth FAKEPASS";
            byte[] expectedResponse = new byte[] { 0x81, 0x02, 0x4F, 0x4B };

            // when
            commandRouter.HandleCommand(command);

            // then
            websocketClient.AdminAuthenticated.Should().Be(true);
            byte[] writes = websocketClient.Stream.GetWrites();
            writes.Should().Equal(expectedResponse);
        }


        [Fact]
        public void ItCanReceiveMessagesSentToSubscribedChannels()
        {
            // given
            WebsocketClient websocketClient = CreateWebsocketClient();
            CommandRouter commandRouter = new CommandRouter(websocketClient);
            WebsocketFrame subscribeFrame = new WebsocketFrame();
            subscribeFrame.opcode = 1;
            subscribeFrame.isMasked = true;
            subscribeFrame.cleartextPayload = Encoding.UTF8.GetBytes("/subscribe channel_1");
            WebsocketFrame publishFrame = new WebsocketFrame();
            publishFrame.opcode = 1;
            publishFrame.isMasked = true;
            publishFrame.cleartextPayload = Encoding.UTF8.GetBytes("/publish channel_1 hello");

            // when
            commandRouter.HandleWebsocketMessage(subscribeFrame);

            // and
            commandRouter.HandleWebsocketMessage(publishFrame);
            websocketClient.Stream.GetWritesAsString().Humanize().Should().Be("hello");
        }

    }
    
}
