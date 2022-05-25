using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;

namespace WebsocketEdu
{
    /// <summary>
    /// This application is a basic example of a working websocket server implementation.  
    /// </summary>
    public class WebsocketEduApp
    {
        static void Main(string[] args)
        {
            string adminPassword = "password";
            SimpleWebsocketServer simpleWebsocketServer = new SimpleWebsocketServer("0.0.0.0", 80, 2, adminPassword);
            simpleWebsocketServer.Start();
        }

    }

}
