using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Discore.Net.Rest
{
    class RestClient : IDisposable
    {
        public const string BASE_URL = "https://discordapp.com/api";

        HttpClient http;
        RestClientRateLimitManager rateLimitManager;
        DiscordApplication app;

        public RestClient(DiscordApplication app)
        {
            this.app = app;

            rateLimitManager = new RestClientRateLimitManager();

            http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "DiscordBot (discore, 2.0)");
            http.DefaultRequestHeaders.Add("Authorization", $"Bot {app.Token}");
        }

        DiscordApiData ParseResponse(HttpResponseMessage response)
        {
            // Read response payload as string
            string json;

            if (response.StatusCode == HttpStatusCode.NoContent)
                json = null;
            else
            {
                try
                {
                    json = response.Content.ReadAsStringAsync().Result;
                }
                catch (AggregateException aex)
                {
                    // Treat async exception as a normal exception.
                    throw aex.InnerException;
                }
            }

            // Attempt to parse the payload as JSON.
            DiscordApiData data;
            if (DiscordApiData.TryCreateFromJson(json, out data))
            {
                if (response.IsSuccessStatusCode)
                    // If successful, no more action is required.
                    return data;
                else
                {
                    // Determine the type of JSON error Discord sent back.
                    IList<DiscordApiData> content = data.GetArray("content");
                    if (content != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < content.Count; i++)
                        {
                            sb.Append(content[i]);

                            if (i < content.Count - 1)
                                sb.Append(", ");
                        }

                        throw new DiscordRestClientException(sb.ToString(), DiscordRestErrorCode.None, response.StatusCode);
                    }
                    else
                    {
                        long code = data.GetInt64("code") ?? 0;
                        string message = data.GetString("message");

                        throw new DiscordRestClientException(message, (DiscordRestErrorCode)code, response.StatusCode);
                    }
                }
            }
            else
                throw new DiscordRestClientException($"Unknown error. Payload: {json}",
                    DiscordRestErrorCode.None, response.StatusCode);
        }

        public DiscordApiData Send(HttpRequestMessage request, string limiterAction)
        {
            try
            {
                rateLimitManager.AwaitRateLimiter(limiterAction);
                HttpResponseMessage response = http.SendAsync(request).Result;
                rateLimitManager.UpdateRateLimiter(limiterAction, response);

                return ParseResponse(response);
            }
            catch (AggregateException aex)
            {
                // Treat async exception as a normal exception.
                throw aex.InnerException;
            }
        }

        public DiscordApiData Get(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            return Send(request, limiterAction);
        }

        public DiscordApiData Post(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction);
        }

        public DiscordApiData Put(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");

            if (data != null)
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction);
        }

        public DiscordApiData Patch(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction);
        }

        public DiscordApiData Delete(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            return Send(request, limiterAction);
        }

        public void Dispose()
        {
            http.Dispose();
        }
    }
}
