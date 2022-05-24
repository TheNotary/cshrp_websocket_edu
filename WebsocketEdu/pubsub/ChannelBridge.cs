using WebsocketEdu.Extensions;

namespace WebsocketEdu
{
    public class ChannelBridge
    {
        //private List<IObserver<string>> observers;
        private Dictionary<string, List<IObserver<string>>> channels;

        public ChannelBridge()
        {
            //this.observers = new List<IObserver<string>>();
            this.channels = new Dictionary<string, List<IObserver<string>>>();
        }

        public IDisposable Subscribe(ChannelSubscriber channelSubscriber, string channel)
        {
            List<IObserver<string>> observers = channels.GetOrCreate(channel);

            if (!observers.Contains(channelSubscriber))
            {
                observers.Add(channelSubscriber);
            }
            return new Unsubscriber<string>(observers, channelSubscriber);
        }

        public void PublishContent(string channel, string content)
        {
            List<IObserver<string>> observers = channels.GetOrCreate(channel);

            foreach (var observer in observers)
                observer.OnNext(content);
        }
    }
}
