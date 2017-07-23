using Nito.AsyncEx;
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
    class RestClient : IDisposable
    {
        public const string BASE_URL = "https://discordapp.com/api/v6";

        public bool RetryOnRateLimit { get; set; } = true;

        static readonly string discoreVersion;

        IDiscordAuthenticator authenticator;
        DiscoreLogger log;

        RateLimitHandlingMethod rateLimitMethod;
        Dictionary<string, RateLimitHandler> rateLimitedRoutes;
        AsyncManualResetEvent globalRateLimitResetEvent;

        HttpClient globalHttpClient;

        public RestClient(IDiscordAuthenticator authenticator, InitialHttpApiSettings settings)
        {
            this.authenticator = authenticator;

            RetryOnRateLimit = settings.RetryWhenRateLimited;
            rateLimitMethod = settings.RateLimitHandlingMethod;

            log = new DiscoreLogger("RestClient");

            if (settings.UseSingleHttpClient)
                globalHttpClient = CreateHttpClient();

            rateLimitedRoutes = new Dictionary<string, RateLimitHandler>();
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

        Task WaitRateLimit(string limiterAction)
        {
            RateLimitHandler routeHandler;
            if (rateLimitedRoutes.TryGetValue(limiterAction, out routeHandler))
                return routeHandler.Wait();

            // Do nothing if no section has been created.
            return Task.CompletedTask;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordApiData> Send(Func<HttpRequestMessage> requestCreate, string limiterAction, 
            CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage response;
            RateLimitHeaders rateLimitHeaders;

            do
            {
                // Wait for global rate limit
                await globalRateLimitResetEvent.WaitAsync().ConfigureAwait(false);
                // Wait for route specific rate limit
                await WaitRateLimit(limiterAction).ConfigureAwait(false);

                // Set to null to make sure the loop doesn't exit with headers from a previous response.
                rateLimitHeaders = null;

                // Send request
                if (globalHttpClient != null)
                {
                    response = await globalHttpClient.SendAsync(requestCreate(), cancellationToken ?? CancellationToken.None)
                        .ConfigureAwait(false);
                }
                else
                {
                    using (HttpClient http = CreateHttpClient())
                    {
                        response = await http.SendAsync(requestCreate(), cancellationToken ?? CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                }

                // Check rate limit values in response if they exist.
                rateLimitHeaders = RateLimitHeaders.ParseOrNull(response.Headers);
                if (rateLimitHeaders != null)
                {
                    if (rateLimitHeaders.IsGlobal)
                    {
                        int retryAfter = rateLimitHeaders.RetryAfter.Value;

                        log.LogWarning($"[{limiterAction}] Hit global rate limit! Blocking all HTTP requests for {retryAfter}ms.");

                        // For global ratelimiting, block all routes.
                        globalRateLimitResetEvent.Reset();

                        await Task.Delay(retryAfter).ConfigureAwait(false);

                        globalRateLimitResetEvent.Set();
                    }
                    else
                    {
                        // Create or get a rate limit handler for this route.
                        RateLimitHandler routeHandler;
                        if (!rateLimitedRoutes.TryGetValue(limiterAction, out routeHandler))
                            rateLimitedRoutes.Add(limiterAction, routeHandler = CreateRateLimitHandler());

                        // Update handler
                        await routeHandler.UpdateValues(rateLimitHeaders).ConfigureAwait(false);

                        // Tell the handler it exceeded rate limits if the status is TooManyRequests.
                        if ((int)response.StatusCode == 429)
                        {
                            routeHandler.ExceededRateLimit(rateLimitHeaders);
                        }
                    }
                }
            }
            // Retry only if the response was 429 and we are allowed to retry.
            while ((int)response.StatusCode == 429 && RetryOnRateLimit);

            return await ParseResponse(response, rateLimitHeaders).ConfigureAwait(false);
        }

        RateLimitHandler CreateRateLimitHandler()
        {
            switch (rateLimitMethod)
            {
                case RateLimitHandlingMethod.Minimal:
                    return new MinimalRateLimitHandler();
                case RateLimitHandlingMethod.Throttle:
                    return new ThrottleRateLimitHandler();
                default:
                    throw new NotImplementedException($"Rate limit handling method: {rateLimitMethod} is not implemented!");
            }
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

        public void Dispose()
        {
            globalHttpClient?.Dispose();

            foreach (RateLimitHandler handler in rateLimitedRoutes.Values)
                handler.Dispose();
        }
    }
}
