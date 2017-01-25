using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
            http.Timeout = TimeSpan.FromMinutes(3);
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

            Task<HttpResponseMessage> sendTask = http.SendAsync(request, cancellationToken ?? CancellationToken.None);

            HttpResponseMessage response;
            try
            {
                response = await sendTask.ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                DateTime afterTask = DateTime.Now;

                DiscoreLogger.Default.LogError($"[RestClient] SendAsync TaskCanceledException: ");
                DiscoreLogger.Default.LogError($"[RestClient] Started At: {beforeTask}");
                DiscoreLogger.Default.LogError($"[RestClient] Ended At: {afterTask}");
                DiscoreLogger.Default.LogError($"[RestClient] HTTP Client Timeout: {http.Timeout}");
                DiscoreLogger.Default.LogError($"[RestClient] ex.CancellationToken.IsCancellationRequested: {ex.CancellationToken.IsCancellationRequested}");
                DiscoreLogger.Default.LogError($"[RestClient] sendTask.IsCanceled: {sendTask.IsCanceled}");
                DiscoreLogger.Default.LogError($"[RestClient] sendTask.IsFaulted: {sendTask.IsFaulted}");
                DiscoreLogger.Default.LogError($"[RestClient] sendTask.Status: {sendTask.Status}");
                throw;
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

        public void Dispose()
        {
            http.Dispose();
        }
    }
}
