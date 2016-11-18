namespace Discore.Http.Net
{
    abstract class HttpApiEndpoint
    {
        protected RestClient Rest { get; }

        protected HttpApiEndpoint(RestClient restClient)
        {
            Rest = restClient;
        }
    }
}
