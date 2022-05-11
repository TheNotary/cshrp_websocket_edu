using System;
using System.IO;
using System.Text;

namespace WebsocketEduTest
{
    public class FeedableMemoryStream : MemoryStream
    {
        long writePosition = 0;
        public FeedableMemoryStream() : base()
        {
        }

        public FeedableMemoryStream(string initialStreamContents)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(initialStreamContents);
            Write(buffer, 0, buffer.Length);
            Seek(0, SeekOrigin.Begin);
            writePosition = buffer.Length + 1;
        }

        public void PutByte(int bits)
        {
            long currentPosition = this.Position;

            // Make sure the write head hasn't fallen behind due to client writes which...
            // should be restricted actually
            if (writePosition < currentPosition) 
                writePosition = currentPosition;

            if (writePosition >= Capacity) // "manually" grow the stream if needed...
            {
                WriteByte((byte) bits);
                Position = currentPosition;   // I don't like how race conditiony this feels
                writePosition = currentPosition + 1;
                return;
            }

            byte[] buffer = GetBuffer();
            SetLength(Length + 1);
            buffer[writePosition] = (byte)bits;
            writePosition++;
        }

        public void PutBytes(byte[] bytes, int v, int length)
        {
            long currentPosition = this.Position;

            for (int i = 0; i < length; i++)
            {
                PutByte(bytes[i]); // Wow I'm lazy...
            }

            this.Position = currentPosition;
        }
    }
}