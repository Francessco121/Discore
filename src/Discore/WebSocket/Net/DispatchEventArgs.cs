namespace Discore.WebSocket.Net
{
    class DispatchEventArgs
    {
        public int Sequence { get; }
        public string EventName { get; }
        public DiscordApiData Data { get; }

        public DispatchEventArgs(int sequence, string eventName, DiscordApiData data)
        {
            Sequence = sequence;
            EventName = eventName;
            Data = data;
        }
    }
}
