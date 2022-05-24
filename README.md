# Websocket Edu

ref:  Websockets generally, https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
ref:  Websockets specifically, https://www.rfc-editor.org/rfc/rfc6455
ref:  parsing a UTF-8 byte stream, https://developpaper.com/c-the-correct-way-to-read-string-from-utf-8-stream/
ref:  Secrets management in VS 2022 https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows


## TODO

x Dry up the code between NetworkStreamProxy and MockNetworkStreamProxy with an abstract class
s Look into a better implementation of the websocket server, streams blow
x Create an object just for WebSocketReader
x Create command for shutdown
- Create command for turning blue
- Make it so the webserver can have two clients communicate to eachother
- Make it so a C# client can connect to the server via SimpleWebsocketClient.Connect("127.0.0.1:80")
- Clean up the server so it looks like SimpleWebsocketServer.Start()
- Put it in a docker container
- Carve it into libraries for use in other projects