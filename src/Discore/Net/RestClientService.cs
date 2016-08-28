using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Net
{
    abstract class RestClientService
    {
        protected readonly DiscordClient client;
        protected readonly RestClient rest;
        protected readonly DiscordApiCacheHelper cacheHelper;

        RestClientRateLimitManager rateLimitManager;
        HttpClient httpClient;

        public RestClientService(DiscordClient client, RestClient rest)
        {
            this.client = client;
            this.rest = rest;

            rateLimitManager = rest.RateLimitManager;
            httpClient = rest.HttpClient;
            cacheHelper = client.CacheHelper;
        }

        DiscordApiData ValidateReturnData(HttpResponseMessage response, DiscordApiData data)
        {
            if (response.IsSuccessStatusCode)
                return data;
            else
            {
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

                    throw new DiscordRestClientException(sb.ToString(), RestErrorCode.BadRequest);
                }
                else
                {
                    long code = data.GetInt64("code") ?? 0;
                    string message = data.GetString("message");

                    throw new DiscordRestClientException(message, (RestErrorCode)code);
                }
            }
        }

        protected async Task<DiscordApiData> Get(string action, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpResponseMessage response = await httpClient.GetAsync($"{RestClient.BASE_URL}/{action}");
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            string json = await response.Content.ReadAsStringAsync();

            DiscordApiData data = DiscordApiData.FromJson(json);

            return ValidateReturnData(response, data);
        }

        protected async Task<DiscordApiData> Post(string action, DiscordApiData data, string limiterAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            return await Post(request, limiterAction);
        }

        protected async Task<DiscordApiData> Post(HttpRequestMessage request, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            string json = await response.Content.ReadAsStringAsync();
            if (json == "")
                return new DiscordApiData(value: null);
            else
            {
                DiscordApiData data = DiscordApiData.FromJson(json);

                return ValidateReturnData(response, data);
            }
        }

        protected async Task<DiscordApiData> Put(string action, DiscordApiData data, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            string json = await response.Content.ReadAsStringAsync();
            DiscordApiData responseData = DiscordApiData.FromJson(json);

            return ValidateReturnData(response, responseData);
        }

        protected async Task<DiscordApiData> Patch(string action, DiscordApiData data, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpMethod method = new HttpMethod("PATCH");
            HttpRequestMessage request = new HttpRequestMessage(method, $"{RestClient.BASE_URL}/{action}");
            request.Content = new StringContent(data.SerializeToJson(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            string json = await response.Content.ReadAsStringAsync();
            DiscordApiData responseData = DiscordApiData.FromJson(json);

            return ValidateReturnData(response, responseData);
        }

        protected async Task<DiscordApiData> Delete(string action, string limiterAction)
        {
            await rateLimitManager.AwaitRateLimiter(limiterAction);

            HttpResponseMessage response = await httpClient.DeleteAsync($"{RestClient.BASE_URL}/{action}");
            rateLimitManager.UpdateRateLimiter(limiterAction, response);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new DiscordApiData(value: null);
            else
            {
                string json = await response.Content.ReadAsStringAsync();
                DiscordApiData data = DiscordApiData.FromJson(json);

                return ValidateReturnData(response, data);
            }
        }
    }
}
