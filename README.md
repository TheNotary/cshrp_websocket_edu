# Websocket Edu

ref:  Websockets generally, https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
ref:  Websockets specifically, https://www.rfc-editor.org/rfc/rfc6455
ref:  parsing a UTF-8 byte stream, https://developpaper.com/c-the-correct-way-to-read-string-from-utf-8-stream/


## TODO

x Dry up the code between NetworkStreamProxy and MockNetworkStreamProxy with an abstract class
- Look into a better implementation of the websocket server, streams blow
- Make it so the webserver can have two clients communicate to eachother
- Create an object just for WebSocketClient
