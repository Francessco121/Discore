using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class RestClient
    {
        public const string BASE_URL = "https://discordapp.com/api";

        IDiscordAuthenticator authenticator;
        RestClientRateLimitManager rateLimitManager;

        static readonly string discoreVersion;

        public RestClient(IDiscordAuthenticator authenticator)
        {
            this.authenticator = authenticator;
            rateLimitManager = new RestClientRateLimitManager();
        }

        static RestClient()
        {
            Version version = Assembly.Load(new AssemblyName("Discore")).GetName().Version;
            // Don't include revision since Discore uses the Major.Minor.Patch semantic.
            discoreVersion = $"{version.Major}.{version.Minor}.{version.Build}";
        }

        HttpClient CreateHttpClient()
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "*/*");
            http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            http.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot (Discore, {discoreVersion})");
            http.DefaultRequestHeaders.Add("Authorization", $"{authenticator.GetTokenHttpType()} {authenticator.GetToken()}");

            return http;
        }

        async Task<DiscordApiData> ParseResponse(HttpResponseMessage response)
        {
            // Read response payload as string
            string json;

            if (response.StatusCode == HttpStatusCode.NoContent)
                json = null;
            else
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

                        throw new DiscordHttpApiException(sb.ToString(), DiscordHttpErrorCode.None, response.StatusCode);
                    }
                    else
                    {
                        long code = data.GetInt64("code") ?? 0;
                        string message = data.GetString("message");

                        throw new DiscordHttpApiException(message, (DiscordHttpErrorCode)code, response.StatusCode);
                    }
                }
            }
            else
                throw new DiscordHttpApiException($"Unknown error. Response: {json}",
                    DiscordHttpErrorCode.None, response.StatusCode);
        }

        public async Task<DiscordApiData> Send(HttpRequestMessage request, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction).ConfigureAwait(false);

            DateTime beforeTask = DateTime.Now;

            HttpResponseMessage response;

            using (HttpClient http = CreateHttpClient())
            {
                Task<HttpResponseMessage> sendTask = http.SendAsync(request, cancellationToken ?? CancellationToken.None);
                response = await sendTask.ConfigureAwait(false);
            }

            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            return await ParseResponse(response).ConfigureAwait(false);
        }

        public Task<DiscordApiData> Get(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Post(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Post(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Put(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Put(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Patch(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return Send(request, limiterAction, cancellationToken);
        }

        public Task<DiscordApiData> Delete(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            return Send(request, limiterAction, cancellationToken);
        }
    }
}
