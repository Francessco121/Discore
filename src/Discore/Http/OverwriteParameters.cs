namespace Discore.Http
{
    /// <summary>
    /// A set of parameters defining a permission overwrite.
    /// </summary>
    public class OverwriteParameters
    {
        /// <summary>
        /// The type of permission overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; set; }
        /// <summary>
        /// Specifically allowed permissions.
        /// </summary>
        public DiscordPermission Allow { get; set; }
        /// <summary>
        /// Specifically denied permissions.
        /// </summary>
        public DiscordPermission Deny { get; set; }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("type", Type.ToString().ToLower());
            data.Set("allow", (int)Allow);
            data.Set("deny", (int)Deny);

            return data;
        }
    }
}
