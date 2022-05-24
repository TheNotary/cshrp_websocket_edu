using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace WebsocketEdu
{
    public class CommandRouter
    {
        WebsocketClient _websocketClient;
        IConfigurationRoot configuration;


        public CommandRouter(WebsocketClient websocketClient)
        {
            _websocketClient = websocketClient;
            configuration = new ConfigurationBuilder().AddUserSecrets<WebsocketEduApp>().Build();
        }

        public CommandRouter(WebsocketClient websocketClient, IConfigurationRoot config)
        {
            _websocketClient = websocketClient;
            configuration = config;
        }

        public void HandleCommand(string msg)
        {
            if (msg == "" || msg.Substring(0, 1) != "/")
                return; // This is not a command message

            string command = Regex.Match(msg, "^/(\\w+)").Value;

            switch (command)
            {
                case ("/close"):
                    if (_websocketClient.AdminAuthenticated)
                        CloseServer();
                    else
                        Console.WriteLine("Not admin, close server request denied");
                    break;
                case ("/auth"):
                    AuthenticateClient(msg);
                    break;
                case ("/subscribe"):
                    SubscribeToChannel(msg);
                    break;
                case ("/publish"):
                    PublishToChannel(msg);
                    break;
                default:
                    Console.WriteLine("Client sent unknown command");
                    break;
            }
        }

        /// <summary>
        /// Use this method to gain "admin" permissions using a password stored in the env.
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AuthenticateClient(string msg)
        {
            string cleanMsg = Regex.Replace(msg, "^/auth", "").Trim();
            string adminPassword = configuration["adminPassword"];

            if (adminPassword == cleanMsg)
            { 
                _websocketClient.AdminAuthenticated = true;
                _websocketClient.SendMessage("OK");
                Console.WriteLine("Admin Password checked to authenticate client");
                return;
            }

            Console.WriteLine("Authentication failure, wrong password");
        }

        private void CloseServer()
        {
            Console.WriteLine("Client sent close command, closing.");
            Environment.Exit(0);
        }

        private void SubscribeToChannel(string command)
        {
            string cleanMsg = Regex.Replace(command, "^/subscribe", "").Trim();
            string channelName = cleanMsg;

            _websocketClient.subscriptions.Add(channelName);

        }

        private void PublishToChannel(string command)
        {
            // split message into parameters
            string cleanParameters = Regex.Replace(command, "^/publish", "").Trim();
            string channelName = cleanParameters.Split(" ")[0];
            string content = Regex.Replace(cleanParameters, $"^{channelName}", "");

            // send message to all client subscribing to that message
            _websocketClient.ChannelBridge.PublishContent(channelName, content);
        }

    }
}
