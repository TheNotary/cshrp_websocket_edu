using System.IO;

namespace WebsocketEduTest
{
    class FeedableMemoryStream : MemoryStream
    {
        long writePosition = 0;

        public void PutByte(int bits)
        {
            long currentPosition = this.Position;
            if (writePosition < currentPosition) // Make sure the write head hasn't fallen behind due to client writes which... should be restricted actually
                writePosition = currentPosition;

            if (writePosition >= this.Capacity) // "manually" grow the stream if needed...
            {
                this.WriteByte((byte) bits);
                this.Position = currentPosition;   // I don't like how race conditiony this feels
                writePosition = currentPosition + 1;
                return;
            }

            byte[] buffer = this.GetBuffer();
            this.SetLength(this.Length + 1);
            buffer[writePosition] = (byte)bits;
            writePosition++;
        }

    }
}