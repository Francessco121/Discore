namespace Discore.Http
{
    /// <summary>
    /// Initial settings to set when creating a DiscordHttpApi object.
    /// Any properties changed in this object after a DiscordHttpApi has been created will not affect it.
    /// </summary>
    public class InitialHttpApiSettings
    {
        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// This is true by default.
        /// </summary>
        public bool RetryWhenRateLimited { get; set; } = true;

        /// <summary>
        /// Gets or sets the method the HTTP API should use when handling API rate limits.
        /// This is set to Throttle by default.
        /// <para>Note: This cannot be changed after.</para>
        /// </summary>
        public RateLimitHandlingMethod RateLimitHandlingMethod { get; set; } = RateLimitHandlingMethod.Throttle;

        /// <summary>
        /// Gets or sets whether a single HTTP client should be used for all API requests.
        /// In rare cases using a single client causes requests to hang until they timeout (believed to be a .NET Core bug).
        /// This is true by default.
        /// <para>Note: This cannot be changed after.</para>
        /// </summary>
        public bool UseSingleHttpClient { get; set; } = true;
    }
}
