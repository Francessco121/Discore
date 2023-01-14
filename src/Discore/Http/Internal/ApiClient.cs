using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Internal
{
    class ApiClient : IDisposable
    {
        public const string BASE_URL = "https://discord.com/api/v10";

        public bool RetryOnRateLimit { get; set; } = true;

        const string DISCORE_URL = "https://github.com/Francessco121/Discore";
        static readonly string discoreVersion;

        static readonly RateLimitLock globalRateLimitLock;
        static readonly ConcurrentDictionary<string, RateLimitLock> routeRateLimitLocks;
        static readonly ConcurrentDictionary<string, string> routesToBuckets;
        static readonly ConcurrentDictionary<string, RateLimitLock> bucketRateLimitLocks;

        readonly HttpClient httpClient;

        public ApiClient(string botToken)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ({DISCORE_URL}, {discoreVersion})");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {botToken}");
        }

        static ApiClient()
        {
            Version version = Assembly.Load(new AssemblyName("Discore")).GetName().Version!;
            // Don't include revision since Discore uses the Major.Minor.Patch semantic.
            discoreVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            globalRateLimitLock = new RateLimitLock();
            routeRateLimitLocks = new ConcurrentDictionary<string, RateLimitLock>();
            routesToBuckets = new ConcurrentDictionary<string, string>();
            bucketRateLimitLocks = new ConcurrentDictionary<string, RateLimitLock>();
        }

        /// <summary>
        /// Note: Consumers of this method become owners of the returned JsonDocument.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        async Task<JsonDocument?> ParseResponse(HttpResponseMessage response, RateLimitHeaders? rateLimitHeaders,
            CancellationToken cancellationToken)
        {
            // Parse response payload as JSON
            JsonDocument? data;

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                // Don't attempt to parse if the response is intentionally empty
                data = null;
            }
            else
            {
                // Parse
                try
                {
                    Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    data = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (JsonException ex)
                {
                    // JSON was expected, but could not be parsed, we should not continue
                    throw new DiscordHttpApiException($"Failed to parse response: {ex}", DiscordHttpErrorCode.None, response.StatusCode, errors: null);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                // Success, no more action is required
                return data;
            }
            else
            {
                // Handle error
                using (data)
                {
                    throw BuildUnsuccessfulResponseException(response, rateLimitHeaders, data!.RootElement);
                }
            }
        }

        DiscordHttpApiException BuildUnsuccessfulResponseException(HttpResponseMessage response, RateLimitHeaders? rateLimitHeaders, JsonElement data)
        {
            // TODO: Parse form error responses: https://discord.com/developers/docs/reference#error-messages

            // Get the Discord-specific error code
            DiscordHttpErrorCode errorCode = DiscordHttpErrorCode.None;

            if ((int)response.StatusCode == 429)
            {
                errorCode = DiscordHttpErrorCode.TooManyRequests;
            }
            else
            {
                int? code = data.GetPropertyOrNull("code")?.GetInt32();

                if (code != null)
                    errorCode = (DiscordHttpErrorCode)code;
            }

            // Get the message
            string message = data.GetPropertyOrNull("message")?.GetString() ?? "An error occurred.";

            // Get the form errors
            DiscordHttpErrorObject? errors = null;
            JsonElement? errorsJson = data.GetPropertyOrNull("errors");

            if (errorsJson != null)
            {
                errors = new DiscordHttpErrorObject(errorsJson.Value);
            }

            // Throw the appropriate exception
            if ((int)response.StatusCode == 429 && rateLimitHeaders != null)
                return new DiscordHttpRateLimitException(rateLimitHeaders, message, errorCode, response.StatusCode, errors);
            else
                return new DiscordHttpApiException(message, errorCode, response.StatusCode, errors);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<JsonDocument?> Send(Func<HttpRequestMessage> requestCreate, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken ct = cancellationToken ?? CancellationToken.None;

            // Get the rate limit lock for the route
            RateLimitLock? routeLock = null;

            if (routesToBuckets.TryGetValue(rateLimitRoute, out string? rateLimitBucket))
            {
                // Use the bucket for the route if it exists
                routeLock = bucketRateLimitLocks[rateLimitBucket];
            }

            if (routeLock == null)
            {
                // Get or create the rate limit lock for the specified route
                if (!routeRateLimitLocks.TryGetValue(rateLimitRoute, out routeLock))
                {
                    routeLock = new RateLimitLock();
                    routeRateLimitLocks[rateLimitRoute] = routeLock;
                }
            }

            // Acquire route-specific lock
            using (await routeLock.LockAsync(ct).ConfigureAwait(false))
            {
                HttpResponseMessage response;
                RateLimitHeaders? rateLimitHeaders;

                bool retry = false;
                int attempts = 0;

                IDisposable? globalLock = null;

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
                        response = await httpClient.SendAsync(requestCreate(), ct).ConfigureAwait(false);

                        // Check rate limit headers
                        rateLimitHeaders = RateLimitHeaders.ParseOrNull(response.Headers);
                        if (rateLimitHeaders != null)
                        {
                            if ((int)response.StatusCode == 429)
                            {
                                // Tell the appropriate lock to wait
                                if (rateLimitHeaders.IsGlobal)
                                    globalRateLimitLock.ResetAfter(rateLimitHeaders.RetryAfter!.Value);
                                else
                                    routeLock.ResetAfter(rateLimitHeaders.ResetAfter!.Value);

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
                                    routeLock.ResetAt(rateLimitHeaders.Reset!.Value);
                            }

                            if (rateLimitHeaders.Bucket != null)
                            {
                                // Create the bucket this route is apparently in if not already created
                                if (!bucketRateLimitLocks.ContainsKey(rateLimitHeaders.Bucket))
                                    bucketRateLimitLocks.TryAdd(rateLimitHeaders.Bucket, routeLock.Clone());

                                // Link the route to the bucket if not already linked
                                routesToBuckets[rateLimitRoute] = rateLimitHeaders.Bucket;
                            }
                            else
                            {
                                // If the route was previously in a rate-limit bucket, but isn't anymore, remove the link
                                if (routesToBuckets.TryRemove(rateLimitRoute, out string? bucket))
                                {
                                    // Additionally remove the bucket if no routes are linked to it
                                    if (!routesToBuckets.Values.Contains(bucket))
                                        bucketRateLimitLocks.TryRemove(bucket, out _);
                                }
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

                return await ParseResponse(response, rateLimitHeaders, ct).ConfigureAwait(false);
            }
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Get(string action, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Post(string action, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Post(string action, string jsonContent, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/{action}");
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Put(string action, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Put(string action, string jsonContent, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{BASE_URL}/{action}");
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Patch(string action, string jsonContent, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BASE_URL}/{action}");
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return request;
            }, rateLimitRoute, cancellationToken);
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<JsonDocument?> Delete(string action, string rateLimitRoute,
            CancellationToken? cancellationToken = null)
        {
            return Send(() =>
            {
                return new HttpRequestMessage(HttpMethod.Delete, $"{BASE_URL}/{action}");
            }, rateLimitRoute, cancellationToken);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
