using System.Collections;
using WebsocketEdu.Extensions;

namespace WebsocketEdu
{
    public class WebsocketSerializer
    {
        private WebsocketFrame frame;

        public WebsocketSerializer(WebsocketFrame frame)
        {
            this.frame = frame;
        }

        public byte[] ToBytes()
        {
            var memoryStream = new MemoryStream();

            // Handle 1st byte: FIN, RSV1-3 (f), and opcode
            
            byte firstByte = SerializeFirstHeaderByte();
            memoryStream.WriteByte(firstByte);

            // Handle 2nd byte: Mask and payload length
            byte secondByte = SerializeSecondHeaderByte();
            memoryStream.WriteByte(secondByte);

            // Handle extended payload length bytes
            byte[] extendedPayloadLengthBytes = SerializeExtendedPayloadLengthBytes();
            memoryStream.Write(extendedPayloadLengthBytes, 0, extendedPayloadLengthBytes.Length);
            
            byte[] maskingKey = GenerateMaskingKey();

            byte[] payloadData = SerializePayloadData();
            

            return memoryStream.ToArray();
        }

        private byte[] SerializePayloadData()
        {
            throw new NotImplementedException();
        }

        private byte[] GenerateMaskingKey()
        {
            throw new NotImplementedException();
        }

        private byte SerializeFirstHeaderByte()
        {
            int header1Left = new BitArray(new bool[4] {
               false, false, false, frame.fin }
            ).ToBytes()[0];
            header1Left = header1Left << 4;
            int joinedHeader1 = header1Left + frame.opcode;
            byte firstByte = BitConverter.GetBytes(joinedHeader1)[0];
            return firstByte;
        }

        private byte SerializeSecondHeaderByte()
        {
            int header2Left = Convert.ToInt32(frame.isMasked) << 7;
            int joinedHeader2 = header2Left + (int) frame.payloadLength;
            return BitConverter.GetBytes(joinedHeader2)[0];
        }

        private byte[] SerializeExtendedPayloadLengthBytes()
        {
            byte[] payloadLengthBytes;
            if (frame.payloadLength >= 126 &&
                frame.payloadLength < 65536)
            {
                // the next 2 bytes are payload length
                payloadLengthBytes = BitConverter.GetBytes(frame.payloadLength).Reverse().ToArray();
            }
            else if (frame.payloadLength == 127)
            {
                joinedHeader2 = header2Left + 127;
                // the next 8 bytes are payload length
                payloadLengthBytes = BitConverter.GetBytes(frame.payloadLength).Reverse().ToArray();
            }
            else if (frame.payloadLength > 18446744073709551615)
            {
                throw new Exception("Attempted to send a payload that was bigger than the max amount.");
            }
            else // < 126
            {
                return new byte[0];
            }

            return payloadLengthBytes;
        }



    }
}
