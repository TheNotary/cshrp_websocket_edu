using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public class ChannelSubscriber : IObserver<string>
    {
        private IDisposable? cancellation;

        public ChannelSubscriber() // TODO: add websocket client here
        {
            
        }

        public virtual void Subscribe(ChannelBridge provider)
        {
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            if (cancellation != null)
                cancellation.Dispose();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string content)
        {
            Console.WriteLine("Message Received so should be relayed: " + content);
        }
    }
}
