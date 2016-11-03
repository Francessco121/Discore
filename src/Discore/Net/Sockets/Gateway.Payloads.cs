using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
{
    partial class Gateway
    {
        delegate void PayloadCallback(DiscordApiData payload, DiscordApiData data);

        Dictionary<GatewayOPCode, PayloadCallback> payloadHandlers;

        void InitializePayloadHandlers()
        {
            payloadHandlers = new Dictionary<GatewayOPCode, PayloadCallback>();
            payloadHandlers[GatewayOPCode.Dispath] = HandleDispatchPayload;
            payloadHandlers[GatewayOPCode.Hello] = HandleHelloPayload;
            payloadHandlers[GatewayOPCode.HeartbeatAck] = HandleHeartbeatAck;
        }

        void HandleDispatchPayload(DiscordApiData payload, DiscordApiData data)
        {
            sequence = payload.GetInteger("s") ?? sequence;
            string eventName = payload.GetString("t");

            DispatchCallback callback;
            if (dispatchHandlers.TryGetValue(eventName, out callback))
                callback(data);
            else
                log.LogWarning($"Missing handler for dispatch event: {eventName}");
        }

        void HandleHelloPayload(DiscordApiData payload, DiscordApiData data)
        {
            heartbeatInterval = data.GetInteger("heartbeat_interval") ?? heartbeatInterval;
            log.LogVerbose($"[HELLO] heartbeat_interval: {heartbeatInterval}ms");
        }

        void HandleHeartbeatAck(DiscordApiData payload, DiscordApiData data)
        {
            // Reset heartbeat timeout
            heartbeatTimeoutAt = Environment.TickCount + (heartbeatInterval * HEARTBEAT_TIMEOUT_MISSED_PACKETS);
        }

        void SendPayload(GatewayOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData(DiscordApiDataType.Container);
            payload.Set("op", (int)op);
            payload.Set("d", data);

            socket.Send(payload.SerializeToJson());
        }

        void SendIdentifyPayload()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", app.Token);
            data.Set("compress", true);
            data.Set("large_threshold", 250);

            if (app.Shards.ShardCount > 1)
            {
                DiscordApiData shardData = new DiscordApiData(DiscordApiDataType.Array);
                shardData.Values.Add(new DiscordApiData(shard.ShardId));
                shardData.Values.Add(new DiscordApiData(app.Shards.ShardCount));
                data.Set("shard", shardData);
            }

            DiscordApiData props = data.Set("properties", new DiscordApiData(DiscordApiDataType.Container));
            props.Set("$os", RuntimeInformation.OSDescription);
            props.Set("$browser", "discore");
            props.Set("$device", "discore");
            props.Set("$referrer", "");
            props.Set("$referring_domain", "");

            SendPayload(GatewayOPCode.Identify, data);
        }

        void SendHeartbeatPayload()
        {
            SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequence));
        }
    }
}
