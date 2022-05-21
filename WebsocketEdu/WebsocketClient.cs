using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu.Extensions;

namespace WebsocketEdu
{
    public class WebsocketClient
    {
        INetworkStream _stream;
        byte[] _headerBytes;
        WebsocketFrame frame;

        public bool AdminAuthenticated { get; set; }

        public INetworkStream Stream { get; }

        public WebsocketClient(INetworkStream stream, byte[] headerBytes)
        {
            _stream = stream;
            _headerBytes = headerBytes;
            frame.fin = (headerBytes[0] & 0b10000000) != 0;
            frame.isMasked = (headerBytes[1] & 0b10000000) != 0;
            frame.opcode = headerBytes[0] & 0b00001111;
            AdminAuthenticated = false;
        }

        public ulong determineMessageLength()
        {
            int msglen = _headerBytes[1] & 0b01111111;
            ulong msglen64 = (ulong)msglen;

            if (msglen == 126) // 126 signifies an extended payload size of 16bits
            {
                byte[] balanceBytes = new byte[2];
                _stream.Read(balanceBytes, 0, balanceBytes.Length);

                msglen64 = BitConverter.ToUInt16(balanceBytes.Reverse().ToArray());
            }
            if (msglen == 127)
            {
                byte[] balanceBytes = new byte[8];
                _stream.Read(balanceBytes, 0, balanceBytes.Length);

                msglen64 = BitConverter.ToUInt64(balanceBytes.Reverse().ToArray());
            }

            Console.WriteLine("Payload length was: " + msglen64.ToString());

            return msglen64;
        }

        /// <summary>
        /// This method will consume the next frame from the stream depending on the headerBytes and the
        ///  length of the websocket frame as determined by parsing the payload
        /// </summary>
        public WebsocketFrame ConsumeFrameFromStream()
        {
            frame.payloadLength = determineMessageLength();
            frame.mask = consumeMask();

            if (frame.payloadLength == 0)
                Console.WriteLine("payloadLength == 0");

            if (frame.isMasked)
            {
                frame.decodedPayload = decodeMessage().ToArray();

                if (frame.opcode == 0x01) // text message
                {
                    return frame;
                }
                if (frame.opcode == 0x08) // close message
                {
                    frame.closeCode = frame.payloadLength >= 2
                        ? BitConverter.ToUInt16(frame.decodedPayload.SubArray(0, 2).Reverse().ToArray())
                        : 0;
                    frame.closeCodeReason = frame.payloadLength > 2
                        ? Encoding.UTF8.GetString(frame.decodedPayload.SubArray(2))
                        : "";
                    return frame;
                }
                throw new Exception("Unknown opcode sent from client, crashing connection");
            }
            else
            {
                byte[] clearText = new byte[frame.payloadLength];
                _stream.Read(clearText, 0, clearText.Length);
                return frame;
            }

        }

        public MemoryStream decodeMessage()
        {
            MemoryStream decodedStream = new MemoryStream();

            for (ulong i = 0; i < frame.payloadLength; i++)
            {
                byte maskI = (byte)frame.mask[i % 4];
                byte rawByte = (byte)_stream.ReadByte();
                byte decodedByte = (byte)(rawByte ^ maskI);
                decodedStream.WriteByte(decodedByte);
            }
            return decodedStream;
        }

        public byte[] BuildCloseFrame(byte[] closeCodeBytes)
        {
            MemoryStream output = new MemoryStream();
            output.WriteByte(0b10001000); // opcode for a finished, closed frame
            output.WriteByte((byte)closeCodeBytes.Length);
            output.Write(closeCodeBytes, 0, closeCodeBytes.Length);
            return output.ToArray();
        }

        /// <summary>
        /// Consumes the next 4 bytes in the stream and returns them as a byte[].  
        /// </summary>
        /// <remarks>
        /// This message assumes the stream cursor is already at the first byte of the mask key
        /// <returns>
        /// The 4 byte mask key that was sent along with the websocket frame under scrutiny.  
        /// If the frame is not masked, will return an empty byte array.
        /// </returns>
        /// </remarks>
        public byte[] consumeMask()
        {
            if (!frame.isMasked)
                return new byte[0];
            byte[] maskingKey = new byte[4];

            _stream.Read(maskingKey, 0, 4);
            return maskingKey;
        }

        public void SendMessage(string msg)
        {
            byte[] payload = Encoding.UTF8.GetBytes(msg);
            WebsocketFrame sendFrame = new WebsocketFrame();
            sendFrame.fin = true;
            sendFrame.opcode = 0x01;
            sendFrame.isMasked = false;
            sendFrame.payloadLength = (ulong) payload.Length;
            sendFrame.decodedPayload = payload;

            WebsocketSerializer serializer = new WebsocketSerializer(sendFrame);





            //_stream.Write();


        }
    }
}
