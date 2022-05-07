using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace WebsocketEduTest
{
    internal class QueueStream : NetworkStream
    {
        private Queue<byte> queue;

        public QueueStream(Socket ignored) : base(ignored)
        {
            queue = new Queue<byte>();
        }

        public override void WriteByte(byte bits)
        {
            queue.Enqueue((byte) bits);
        }

        public override int ReadByte()
        {
            return (int) queue.Dequeue();
        }

    }
}