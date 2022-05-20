# Websocket Edu

ref:  Websockets generally, https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
ref:  Websockets specifically, https://www.rfc-editor.org/rfc/rfc6455
ref:  parsing a UTF-8 byte stream, https://developpaper.com/c-the-correct-way-to-read-string-from-utf-8-stream/
ref:  Secrets management in VS 2022 https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows


## TODO

x Dry up the code between NetworkStreamProxy and MockNetworkStreamProxy with an abstract class
s Look into a better implementation of the websocket server, streams blow
x Create an object just for WebSocketReader
- Make it so the client can send a command to the websocket server for turn blue and shutdown
- Make it so the webserver can have two clients communicate to eachother
