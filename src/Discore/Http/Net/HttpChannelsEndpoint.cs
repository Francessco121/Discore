using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpChannelsEndpoint : HttpApiEndpoint
    {
        public HttpChannelsEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public DiscordApiData Create(Snowflake channelId, string content, bool? tts = null, Snowflake? nonce = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", content);
            data.Set("tts", tts);
            data.Set("nonce", nonce);

            return Rest.Post($"channels/{channelId}/messages", data, "CreateMessage");
        }
    }
}
