namespace WebsocketEdu
{
    public class ChannelBridge
    {
        private List<IObserver<string>> observers;

        public ChannelBridge()
        {
            this.observers = new List<IObserver<string>>();
        }

        public IDisposable Subscribe(ChannelSubscriber channelSubscriber)
        {
            if (!observers.Contains(channelSubscriber))
            {
                observers.Add(channelSubscriber);
                //// Provide observer with existing data.
                //foreach (var item in flights)
                //    channelSubscriber.OnNext(item);
            }
            return new Unsubscriber<string>(observers, channelSubscriber);
        }

        public void PublishContent(string channel, string content)
        {
            foreach (var observer in observers)
                observer.OnNext(content);

        }
    }
}
