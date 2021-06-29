using System;

namespace Discore
{
    /// <summary>
    /// A URL builder for Discord CDN resources.
    /// </summary>
    public class DiscordCdnUrl : IEquatable<DiscordCdnUrl?>
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
        /// Gets the ID of the resource this URL is for (e.g. user ID, guild ID, etc.)
        /// or null if there is no resource ID (e.g. a default user avatar).
        /// </summary>
        public Snowflake? ResourceId { get; }
        /// <summary>
        /// Gets the original file name provided by the API. This is usually
        /// a hash of the resource.
        /// </summary>
        public string FileName { get; }

        readonly string baseUrl;

        private DiscordCdnUrl(DiscordCdnUrlType type, Snowflake? resourceId, string fileName,
            string baseUrl)
        {
            Type = type;
            ResourceId = resourceId;
            FileName = fileName;

            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// Creates a CDN URL builder for custom emojis.
        /// </summary>
        /// <param name="emojiId">The ID of the custom emoji.</param>
        public static DiscordCdnUrl ForCustomEmoji(Snowflake emojiId)
        {
            return new DiscordCdnUrl(DiscordCdnUrlType.CustomEmoji, emojiId, emojiId.ToString(),
                $"{CdnBaseUrl}/emojis/{emojiId}");
        }

        /// <summary>
        /// Creates a CDN URL builder for guild icons.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="iconHash">The icon hash for the guild.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="iconHash"/> is null.</exception>
        public static DiscordCdnUrl ForGuildIcon(Snowflake guildId, string iconHash)
        {
            if (iconHash == null) throw new ArgumentNullException(nameof(iconHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.GuildIcon, guildId, iconHash,
                $"{CdnBaseUrl}/icons/{guildId}/{iconHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for guild splashes.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="splashHash">The hash of the splash image for the guild.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="splashHash"/> is null.</exception>
        public static DiscordCdnUrl ForGuildSplash(Snowflake guildId, string splashHash)
        {
            if (splashHash == null) throw new ArgumentNullException(nameof(splashHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.GuildSplash, guildId, splashHash,
                $"{CdnBaseUrl}/splashes/{guildId}/{splashHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for guild banners.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="bannerHash">The hash of the banner image for the guild.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bannerHash"/> is null.</exception>
        public static DiscordCdnUrl ForGuildBanner(Snowflake guildId, string bannerHash)
        {
            if (bannerHash == null) throw new ArgumentNullException(nameof(bannerHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.GuildBanner, guildId, bannerHash,
                $"{CdnBaseUrl}/banners/{guildId}/{bannerHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a default user avatar.
        /// </summary>
        /// <param name="userDiscriminator">The original user discriminator.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userDiscriminator"/> is null.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="userDiscriminator"/> is not a valid integer.</exception>
        /// <exception cref="OverflowException">
        /// Thrown if <paramref name="userDiscriminator"/> represents a number less than <see cref="int.MinValue"/>
        /// or greater than <see cref="int.MaxValue"/>.
        /// </exception>
        public static DiscordCdnUrl ForDefaultUserAvatar(string userDiscriminator)
        {
            if (userDiscriminator == null) throw new ArgumentNullException(nameof(userDiscriminator));

            // The actual file name is the original discriminator modulo 5.
            int discriminatorNum = int.Parse(userDiscriminator);
            string fileName = (discriminatorNum % 5).ToString();

            return new DiscordCdnUrl(DiscordCdnUrlType.DefaultUserAvatar, null, fileName,
                $"{CdnBaseUrl}/embed/avatars/{fileName}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a user avatar.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="avatarHash">The avatar hash for the user.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="avatarHash"/> is null.</exception>
        public static DiscordCdnUrl ForUserAvatar(Snowflake userId, string avatarHash)
        {
            if (avatarHash == null) throw new ArgumentNullException(nameof(avatarHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.UserAvatar, userId, avatarHash,
                $"{CdnBaseUrl}/avatars/{userId}/{avatarHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for an application icon.
        /// </summary>
        /// <param name="applicationId">The ID of the application.</param>
        /// <param name="iconHash">The icon hash for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="iconHash"/> is null.</exception>
        public static DiscordCdnUrl ForApplicationIcon(Snowflake applicationId, string iconHash)
        {
            if (iconHash == null) throw new ArgumentNullException(nameof(iconHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.ApplicationIcon, applicationId, iconHash,
                $"{CdnBaseUrl}/app-icons/{applicationId}/{iconHash}");
        }

        /// <summary>
        /// Gets the complete URL with the specified extension and size.
        /// </summary>
        /// <param name="ext">The resource file extension (e.g. png, webp, gif, etc.).</param>
        /// <param name="size">
        /// <para>An optional pixel size of the resource to return (sets both width and height).</para>
        /// <para>Note: Must be a power of 2 and be between 16 and 2048.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ext"/> is null.</exception>
        public string BuildUrl(string ext = "png", int? size = null)
        {
            if (ext == null) throw new ArgumentNullException(nameof(ext));

            if (size.HasValue)
                return $"{baseUrl}.{ext}?size={size.Value}";
            else
                return $"{baseUrl}.{ext}";
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DiscordCdnUrl);
        }

        public bool Equals(DiscordCdnUrl? other)
        {
            return !(other is null) &&
                   Type == other.Type &&
                   ResourceId.Equals(other.ResourceId) &&
                   FileName == other.FileName &&
                   baseUrl == other.baseUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResourceId, FileName, baseUrl);
        }

        public static bool operator ==(DiscordCdnUrl? left, DiscordCdnUrl? right)
        {
            if (left is null != right is null) return true;
            if (left is null) return false;

            return left.Equals(right);
        }

        public static bool operator !=(DiscordCdnUrl? left, DiscordCdnUrl? right)
        {
            return !(left == right);
        }
    }
}
