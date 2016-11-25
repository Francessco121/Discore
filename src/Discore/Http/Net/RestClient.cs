using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class RestClient : IDisposable
    {
        public const string BASE_URL = "https://discordapp.com/api";

        HttpClient http;
        RestClientRateLimitManager rateLimitManager;

        public RestClient(IDiscordAuthenticator authenticator)
        {
            rateLimitManager = new RestClientRateLimitManager();

            http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "DiscordBot (discore, 2.0)");
            http.DefaultRequestHeaders.Add("Authorization", $"{authenticator.GetTokenHttpType()} {authenticator.GetToken()}");
        }

        async Task<DiscordApiData> ParseResponse(HttpResponseMessage response)
        {
            // Read response payload as string
            string json;

            if (response.StatusCode == HttpStatusCode.NoContent)
                json = null;
            else
                json = await response.Content.ReadAsStringAsync();

            // Attempt to parse the payload as JSON.
            DiscordApiData data;
            if (DiscordApiData.TryParseJson(json, out data))
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

                        throw new DiscordHttpClientException(sb.ToString(), DiscordHttpErrorCode.None, response.StatusCode);
                    }
                    else
                    {
                        long code = data.GetInt64("code") ?? 0;
                        string message = data.GetString("message");

                        throw new DiscordHttpClientException(message, (DiscordHttpErrorCode)code, response.StatusCode);
                    }
                }
            }
            else
                throw new DiscordHttpClientException($"Unknown error. Response: {json}",
                    DiscordHttpErrorCode.None, response.StatusCode);
        }

        public async Task<DiscordApiData> Send(HttpRequestMessage request, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpResponseMessage response = await http.SendAsync(request);
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            return await ParseResponse(response);
        }

        public async Task<DiscordApiData> Get(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Post(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Post(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Put(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Put(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Patch(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return await Send(request, limiterAction);
        }

        public async Task<DiscordApiData> Delete(string action, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            return await Send(request, limiterAction);
        }

        public void Dispose()
        {
            http.Dispose();
        }
    }
}
