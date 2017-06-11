using System;

namespace Discore
{
    /// <summary>
    /// A URL builder for Discord CDN resources.
    /// </summary>
    public class DiscordCdnUrl
    {
        /// <summary>
        /// The base URL for all Discord CDN resources.
        /// </summary>
        public const string CdnBaseUrl = "https://cdn.discordapp.com";

        /// <summary>
        /// Gets the type of resource this URL is pointing to.
        /// </summary>
        public DiscordCdnUrlType Type { get; }
        /// <summary>
        /// Gets the ID of the resource this URL is for (e.g. user ID, guild ID, etc.).
        /// </summary>
        public Snowflake ResourceId { get; }
        /// <summary>
        /// Gets the original resource hash provided by the API.
        /// </summary>
        public string Hash { get; }

        string typeStr;

        /// <summary>
        /// Creates a URL builder for any given resource for Discord's CDN.
        /// </summary>
        /// <param name="type">The type of resource.</param>
        /// <param name="resourceId">The ID of the resource.</param>
        /// <param name="hash">The hash of the resource (this is normally provided by the API).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hash"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="type"/> is invalid.</exception>
        public DiscordCdnUrl(DiscordCdnUrlType type, Snowflake resourceId, string hash)
        {
            Type = type;
            ResourceId = resourceId;
            Hash = hash;

            switch (type)
            {
                case DiscordCdnUrlType.Avatar:
                    typeStr = "avatars";
                    break;
                case DiscordCdnUrlType.Icon:
                    typeStr = "icons";
                    break;
                case DiscordCdnUrlType.Splash:
                    typeStr = "splashes";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Gets the complete URL.
        /// </summary>
        /// <param name="ext">The resource file extension (e.g. png, webp, etc.).</param>
        /// <param name="size">An optional pixel size of the resource to return (sets both width and height).</param>
        /// <returns></returns>
        public string BuildUrl(string ext = "png", int? size = null)
        {
            if (size.HasValue)
                return $"{CdnBaseUrl}/{typeStr}/{ResourceId}/{Hash}.{ext}?size={size.Value}";
            else
                return $"{CdnBaseUrl}/{typeStr}/{ResourceId}/{Hash}.{ext}";
        }
    }
}
