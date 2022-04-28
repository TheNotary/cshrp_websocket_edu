// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;


static void Main(string[] args)
{

    // Console.WriteLine(new string('*', 65535) + "hello");

    TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 80);

    server.Start();
    Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);


    TcpClient client = server.AcceptTcpClient();
    Console.WriteLine("A client connected.\n");

    NetworkStream stream = client.GetStream();

    //enter to an infinite cycle to be able to handle every change in stream
    while (true)
    {
        while (!stream.DataAvailable) ;

        WaitForMoreClientData(client, stream);
    }

}


static void WaitForMoreClientData(TcpClient client, NetworkStream stream)
{
    // 1.
    // wait for the first 3 bytes to be available.  Websocket messages consist of a three byte header detailing 
    // the shape of the incoming websocket frame
    while (client.Available < 3); 

    // 2. 
    // Get the client's data now that they've at least gotten to the "GET" part or the frame header
    Byte[] bytes = new Byte[client.Available];      // change this to 3 bytes exactly
    stream.Read(bytes, 0, bytes.Length);
    String data = Encoding.UTF8.GetString(bytes);

    Console.WriteLine("New Bytes ready for processing from client: " + bytes.Length);

    if ( HandleHandshake(stream, data) ) return;

    // Handle ordinary communication
    HandleMessage(stream, bytes);

    // Console.WriteLine("Client: " + data);
}

static void HandleMessage(NetworkStream stream, Byte[] bytes)
{
    bool fin = (bytes[0] & 0b10000000) != 0,
         mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"

    int opcode = bytes[0] & 0b00001111; // expecting 0x0001, indicating a text message


    // determine the message length
    object[] result = determineMessageLength(bytes);
    ulong msglen = (ulong) result[0];
    int offset = (int) result[1];

    if (msglen == 0)
        Console.WriteLine("msglen == 0");
    else if (mask)
    {
        Byte[] decoded = decodeMessage(bytes, offset);    // only has up to 65521 properly decoded, the rest are empty bytes...
        string text = Encoding.UTF8.GetString(decoded);
        Console.WriteLine("Client: {0}", text);
    }
    else { 
        Console.WriteLine("mask bit not set");
        string text = Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
        Console.WriteLine("Client: {0}", text);
    }

}

static object[] determineMessageLength(Byte[] bytes)
{
    int msglen = bytes[1] & 0b01111111;
    int offset = 2;
    ulong msglen64 = (ulong) msglen;

    if (msglen == 126)// 126 signifies an extended payload size of 16bits
    {
        // We could use the ToUInt16, but it's less supported across frameworks than just using longs which are pretty big
        msglen64 = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] });
        offset = 4;
    } 
    if (msglen == 127)
    {
        msglen64 = BitConverter.ToUInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] });
        offset = 10;
        Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
        // i don't really know the byte order, please edit this
        // msglen = BitConverter.ToUInt64(new byte[] { data[5], data[4], data[3], data[2], data[9], data[8], data[7], data[6] }, 0);
        // offset = 10;
    }

    

    return new object[]{ msglen64, offset };
}


static byte[] decodeMessage(Byte[] bytes, int offset)
{
    Byte[] decoded = new Byte[bytes.Length];  // TODO, shouldn't this be bytes - offset actually?

    // read mask, has an offset of 2 or 4 at this point depending on... something to do with ... chance?
    byte[] mask = new byte[4] { bytes[offset], 
                                bytes[offset + 1], 
                                bytes[offset + 2], 
                                bytes[offset + 3] };


    offset += 4;
    for (int i = 0; i < bytes.Length - offset; i++)
    {
        decoded[i] = (Byte)(bytes[offset + i] ^ mask[i % 4]);
    }
    return decoded;
}

static bool HandleHandshake(NetworkStream stream, string data)
{
    if (!Regex.IsMatch(data, "^GET"))
        return false;

    string webSocketKey = GenerateWebsocketHeaderValue(data);
    RespondToHandshake(stream, webSocketKey);
    return true;
}


static void RespondToHandshake(NetworkStream stream, string webSocketHeader)
{
    const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
        + "Connection: Upgrade" + eol
        + "Upgrade: websocket" + eol
        + "Sec-WebSocket-Accept: " + webSocketHeader + eol
        + eol);
    stream.Write(response, 0, response.Length);
}


static string GenerateWebsocketHeaderValue(String data)
{
    string webSocketKey = new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim();
    return Convert.ToBase64String(
            SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))
        );
}



Main(args);
