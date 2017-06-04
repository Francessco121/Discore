using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class RestClient : IDisposable
    {
        public const string BASE_URL = "https://discordapp.com/api";

        public bool RetryOnRateLimit { get; set; } = true;

        const string DISCORE_URL = "https://github.com/BundledSticksInkorperated/Discore";
        static readonly string discoreVersion;

        string botToken;
        DiscoreLogger log;

        static RateLimitLock globalRateLimitLock;
        static ConcurrentDictionary<string, RateLimitLock> routeRateLimitLocks;

        HttpClient globalHttpClient;

        public RestClient(string botToken)
        {
            this.botToken = botToken;

            log = new DiscoreLogger("RestClient");

            if (DiscordHttpClient.UseSingleHttpClient)
                globalHttpClient = CreateHttpClient();
        }

        static RestClient()
        {
            Version version = Assembly.Load(new AssemblyName("Discore")).GetName().Version;
            // Don't include revision since Discore uses the Major.Minor.Patch semantic.
            discoreVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            globalRateLimitLock = new RateLimitLock();
            routeRateLimitLocks = new ConcurrentDictionary<string, RateLimitLock>();
        }

        HttpClient CreateHttpClient()
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "*/*");
            http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            http.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ({DISCORE_URL}, {discoreVersion})");
            http.DefaultRequestHeaders.Add("Authorization", $"Bot {botToken}");

            return http;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        async Task<DiscordApiData> ParseResponse(HttpResponseMessage response, RateLimitHeaders rateLimitHeaders)
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
                    string message = null;
                    DiscordHttpErrorCode errorCode = DiscordHttpErrorCode.None;

                    // Get the Discord-specific error code if it exists.
                    if ((int)response.StatusCode == 429)
                        errorCode = DiscordHttpErrorCode.TooManyRequests;
                    else if (data.ContainsKey("code"))
                    {
                        long? code = data.GetInt64("code");
                        if (code.HasValue)
                            errorCode = (DiscordHttpErrorCode)code;
                    }

                    // Get the message.
                    if (data.ContainsKey("content"))
                    {
                        IList<DiscordApiData> content = data.GetArray("content");

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < content.Count; i++)
                        {
                            sb.Append(content[i]);

                            if (i < content.Count - 1)
                                sb.Append(", ");
                        }

                        message = sb.ToString();
                    }
                    else if (data.ContainsKey("message"))
                    {
                        message = data.GetString("message");
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest && data.Type == DiscordApiDataType.Container)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (KeyValuePair<string, DiscordApiData> pair in data.Entries)
                        {
                            sb.Append($"{pair.Key}: ");

                            if (pair.Value.Type != DiscordApiDataType.Array)
                            {
                                // Shouldn't happen, but if it does then this error is not one we can parse.
                                // Set sb to null so that message ends up null and let the application get
                                // the raw JSON payload so they at least know what happened until we can
                                // implement the correct parser.
                                sb = null;
                                break;
                            }

                            bool addComma = false;
                            foreach (DiscordApiData errorData in pair.Value.Values)
                            {
                                if (addComma)
                                    sb.Append(", ");

                                sb.Append(errorData.ToString());

                                addComma = true;
                            }

                            sb.AppendLine();
                        }

                        message = sb?.ToString();
                    }

                    // Throw the appropriate exception
                    if (message != null) // If null, let the "unknown error" exception be thrown
                    {
                        if ((int)response.StatusCode == 429 && rateLimitHeaders != null)
                            throw new DiscordHttpRateLimitException(rateLimitHeaders, message, errorCode, response.StatusCode);
                        else
                            throw new DiscordHttpApiException(message, errorCode, response.StatusCode);
                    }
                }
            }

            throw new DiscordHttpApiException($"Unknown error. Response: {json}",
                DiscordHttpErrorCode.None, response.StatusCode);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordApiData> Send(Func<HttpRequestMessage> requestCreate, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            CancellationToken ct = cancellationToken ?? CancellationToken.None;

            // Get or create the rate limit lock for the specified route
            RateLimitLock routeLock;
            if (!routeRateLimitLocks.TryGetValue(rateLimitRoute, out routeLock))
            {
                routeLock = new RateLimitLock();
                routeRateLimitLocks[rateLimitRoute] = routeLock;
            }

            // Acquire route-specific lock
            using (await routeLock.LockAsync(ct).ConfigureAwait(false))
            {
                HttpResponseMessage response;
                RateLimitHeaders rateLimitHeaders;

                bool retry = false;
                int attempts = 0;

                IDisposable globalLock = null;

                do
                {
                    retry = false;
                    attempts++;

                    // If the route-specific lock requires a wait, delay before continuing
                    await routeLock.WaitAsync(ct).ConfigureAwait(false);
                    
                    // If we don't already have the global lock and the global rate limit is active, acquire it.
                    if (globalLock == null && globalRateLimitLock.RequiresWait)
                        globalLock = await globalRateLimitLock.LockAsync(ct).ConfigureAwait(false);

                    bool keepGlobalLock = false;

                    try
                    {
                        // If we have the global lock, delay if it requires a wait
                        if (globalLock != null)
                            await globalRateLimitLock.WaitAsync(ct).ConfigureAwait(false);

                        // Send request
                        if (globalHttpClient != null)
                        {
                            response = await globalHttpClient.SendAsync(requestCreate(), ct).ConfigureAwait(false);
                        }
                        else
                        {
                            using (HttpClient http = CreateHttpClient())
                            {
                                response = await http.SendAsync(requestCreate(), ct).ConfigureAwait(false);
                            }
                        }

                        // Check rate limit headers
                        rateLimitHeaders = RateLimitHeaders.ParseOrNull(response.Headers);
                        if (rateLimitHeaders != null)
                        {
                            if ((int)response.StatusCode == 429)
                            {
                                // Tell the appropriate lock to wait
                                if (rateLimitHeaders.IsGlobal)
                                    globalRateLimitLock.ResetAfter(rateLimitHeaders.RetryAfter.Value);
                                else
                                    routeLock.ResetAfter(rateLimitHeaders.RetryAfter.Value);

                                retry = RetryOnRateLimit && attempts < 20;

                                // If we are retrying from a global rate limit, don't release the lock
                                // so this request doesn't go to the back of the queue.
                                keepGlobalLock = retry && rateLimitHeaders.IsGlobal;
                            }
                            else
                            {
                                // If the request succeeded but we are out of calls, set the route lock
                                // to wait until the reset time.
                                if (rateLimitHeaders.Remaining == 0)
                                    routeLock.ResetAt(rateLimitHeaders.Reset);
                            }
                        }

                        // Check if the request received a bad gateway
                        if (response.StatusCode == HttpStatusCode.BadGateway)
                            retry = attempts < 10;

                        // Release the global lock if necessary
                        if (globalLock != null && !keepGlobalLock)
                            globalLock.Dispose();
                    }
                    catch
                    {
                        // Always release the global lock if we ran into an exception
                        globalLock?.Dispose();

                        // Don't suppress the exception
                        throw;
                    }
                }
                while (retry);

                return await ParseResponse(response, rateLimitHeaders).ConfigureAwait(false);
            }
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Get(string action, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Post(string action, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Post(string action, DiscordApiData data, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Put(string action, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Put(string action, DiscordApiData data, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Patch(string action, DiscordApiData data, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
                request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordApiData> Delete(string action, string rateLimitRoute, 
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        public void Dispose()
        {
            globalHttpClient?.Dispose();
        }
    }
}
