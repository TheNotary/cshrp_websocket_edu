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
                case ("/1 "):
                    RelayMessageTo(1, msg.Substring(2).Trim());
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
                Console.WriteLine("Admin Password checked to authenticate client");
                return;
            }

            Console.WriteLine("Authentication failure, wrong password");
        }

        private void CloseServer()
        {
            Console.WriteLine("Client sent close command, closing");
            Environment.Exit(0);
        }

        private void RelayMessageTo(int v1, string v2)
        {
            // Need to Write via my websocket client class... 
            throw new NotImplementedException();
        }

        

    }
}
