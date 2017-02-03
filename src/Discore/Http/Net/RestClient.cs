using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool RetryOnRateLimit { get; set; } = true;

        static readonly string discoreVersion;

        IDiscordAuthenticator authenticator;
        DiscoreLogger log;
        Dictionary<string, RateLimitRoute> rateLimitRoutes;
        AsyncManualResetEvent globalRateLimitResetEvent;

        public RestClient(IDiscordAuthenticator authenticator)
        {
            this.authenticator = authenticator;

            log = new DiscoreLogger("RestClient");

            rateLimitRoutes = new Dictionary<string, RateLimitRoute>();
            globalRateLimitResetEvent = new AsyncManualResetEvent(true);
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

        /// <exception cref="DiscordHttpApiException"></exception>
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

        Task WaitRateLimit(string limiterAction)
        {
            RateLimitRoute route;
            if (rateLimitRoutes.TryGetValue(limiterAction, out route))
                return route.Wait();

            // Do nothing if no section has been created.
            return Task.CompletedTask;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordApiData> Send(Func<HttpRequestMessage> requestCreate, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage response;

            do
            {
                await globalRateLimitResetEvent.WaitAsync().ConfigureAwait(false);
                await WaitRateLimit(limiterAction).ConfigureAwait(false);

                using (HttpClient http = CreateHttpClient())
                {
                    response = await http.SendAsync(requestCreate(), cancellationToken ?? CancellationToken.None)
                        .ConfigureAwait(false);
                }

                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // Create or get limit section if this endpoint uses rate limits.
                    RateLimitRoute route;
                    if (!rateLimitRoutes.TryGetValue(limiterAction, out route))
                        rateLimitRoutes.Add(limiterAction, route = new RateLimitRoute(globalRateLimitResetEvent));

                    await route.Update(response.Headers).ConfigureAwait(false);

                    // Tell the section it exceeded rate limits if the status is TooManyRequests.
                    if ((int)response.StatusCode == 429)
                        route.ExceededRateLimit(response.Headers);
                }

                if (response.Headers.Contains("X-RateLimit-Global"))
                {
                    log.LogWarning($"[{limiterAction}] Hit global rate limit.");

                    // For global ratelimiting, block all limiters.
                    globalRateLimitResetEvent.Reset();

                    IEnumerable<string> retryAfterValues;
                    if (response.Headers.TryGetValues("Retry-After", out retryAfterValues))
                    {
                        string retryAfterStr = retryAfterValues.FirstOrDefault();

                        int retryAfter;
                        if (!string.IsNullOrWhiteSpace(retryAfterStr) && int.TryParse(retryAfterStr, out retryAfter))
                        {
                            await Task.Delay(retryAfter).ConfigureAwait(false);
                        }
                    }

                    globalRateLimitResetEvent.Set();
                }
            }
            while ((int)response.StatusCode == 429 && RetryOnRateLimit);

            return await ParseResponse(response).ConfigureAwait(false);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Get(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Post(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Post(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Put(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Put(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Patch(string action, DiscordApiData data, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, limiterAction, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Delete(string action, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            }, limiterAction, cancellationToken);
        }
    }
}
