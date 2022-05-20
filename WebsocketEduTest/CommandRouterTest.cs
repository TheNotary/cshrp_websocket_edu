using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;
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

            // when
            commandRouter.HandleCommand(command);

            // then
            Assert.True(websocketClient.AdminAuthenticated);
        }

    }
    
}
