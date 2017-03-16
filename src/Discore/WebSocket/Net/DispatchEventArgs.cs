namespace Discore.WebSocket.Net
{
    class DispatchEventArgs
    {
        public string EventName { get; }
        public DiscordApiData Data { get; }

        public DispatchEventArgs(string eventName, DiscordApiData data)
        {
            EventName = eventName;
            Data = data;
        }
    }
}
