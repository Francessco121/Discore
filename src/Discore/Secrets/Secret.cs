using System.ComponentModel;
using Newtonsoft.Json;

namespace Discore.Secrets
{
    /// <summary>
    /// Class reflecting the Json structure of <see cref="Secret"/>
    /// </summary>
    public struct Secret
    {
        [JsonRequired]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DefaultValue(false)]
        [JsonProperty("default",
            DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Default { get; set; }
        [JsonRequired]
        [JsonProperty("config")]
        public SecretConfig Config { get; set; }
    }

    /// <summary>
    /// Class reflecting the Json structure of <see cref="SecretConfig"/>
    /// </summary>
    public struct SecretConfig
    {
        [JsonRequired]
        [JsonProperty("tokens")]
        public SecretToken[] Tokens { get; set; }
    }

    /// <summary>
    /// Class reflecting the Json structure of <see cref="SecretToken"/>
    /// </summary>
    public struct SecretToken
    {
        [JsonRequired]
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonRequired()]
        [JsonProperty("debug")]
        public bool Debug { get; set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}