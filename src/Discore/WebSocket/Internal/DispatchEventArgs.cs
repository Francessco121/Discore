using System.Text.Json;

#nullable enable

namespace Discore.WebSocket.Internal
{
    class DispatchEventArgs
    {
        public string EventName { get; }
        public JsonElement Data { get; }

        public DispatchEventArgs(string eventName, JsonElement data)
        {
            EventName = eventName;
            Data = data;
        }
    }
}

#nullable restore
