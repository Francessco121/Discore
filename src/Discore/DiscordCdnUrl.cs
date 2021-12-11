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
        /// <summary>
        /// Gets the computed base URL for any image derived from this CDN URL
        /// (e.g. "avatars/{userId}/{avatarHash}").
        /// <para/>
        /// Does not include <see cref="CdnBaseUrl"/>.
        /// </summary>
        public string BaseUrl { get; }

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fileName"/> or <paramref name="baseUrl"/> is null.
        /// </exception>
        private DiscordCdnUrl(
            DiscordCdnUrlType type, 
            Snowflake? resourceId, 
            string fileName,
            string baseUrl)
        {
            Type = type;
            ResourceId = resourceId;
            FileName = fileName ?? throw new ArgumentNullException(fileName);
            BaseUrl = baseUrl ?? throw new ArgumentNullException(fileName);
        }

        /// <summary>
        /// Creates a CDN URL builder for custom emojis.
        /// </summary>
        /// <param name="emojiId">The ID of the custom emoji.</param>
        public static DiscordCdnUrl ForCustomEmoji(Snowflake emojiId)
        {
            return new DiscordCdnUrl(DiscordCdnUrlType.CustomEmoji, emojiId, emojiId.ToString(),
                $"emojis/{emojiId}");
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
                $"icons/{guildId}/{iconHash}");
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
                $"splashes/{guildId}/{splashHash}");
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
                $"banners/{guildId}/{bannerHash}");
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
                $"embed/avatars/{fileName}");
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
                $"avatars/{userId}/{avatarHash}");
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
                $"app-icons/{applicationId}/{iconHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for an application cover.
        /// </summary>
        /// <param name="applicationId">The ID of the application.</param>
        /// <param name="coverHash">The cover hash for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="coverHash"/> is null.</exception>
        public static DiscordCdnUrl ForApplicationCover(Snowflake applicationId, string coverHash)
        {
            if (coverHash == null) throw new ArgumentNullException(nameof(coverHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.ApplicationCover, applicationId, coverHash,
                $"{CdnBaseUrl}/app-icons/{applicationId}/{coverHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for an application asset.
        /// </summary>
        /// <param name="applicationId">The ID of the application.</param>
        /// <param name="assetId">The application asset ID.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assetId"/> is null.</exception>
        public static DiscordCdnUrl ForApplicationAsset(Snowflake applicationId, string assetId)
        {
            if (assetId == null) throw new ArgumentNullException(nameof(assetId));

            return new DiscordCdnUrl(DiscordCdnUrlType.ApplicationAsset, applicationId, assetId,
                $"{CdnBaseUrl}/app-assets/{applicationId}/{assetId}");
        }

        /// <summary>
        /// Creates a CDN URL builder for an achievement icon.
        /// </summary>
        /// <param name="applicationId">The ID of the application.</param>
        /// <param name="achievementId">The ID of the achievement.</param>
        /// <param name="iconHash">The hash for the icon.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="iconHash"/> is null.</exception>
        public static DiscordCdnUrl ForAchievementIcon(Snowflake applicationId, long achievementId, string iconHash)
        {
            if (iconHash == null) throw new ArgumentNullException(nameof(iconHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.AchievementIcon, applicationId, iconHash,
                $"{CdnBaseUrl}/app-assets/{applicationId}/achievements/{achievementId}/icons/{iconHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a sticker pack banner.
        /// </summary>
        /// <param name="assetId">The asset ID.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assetId"/> is null.</exception>
        public static DiscordCdnUrl ForStickerPackBanner(string assetId)
        {
            if (assetId == null) throw new ArgumentNullException(nameof(assetId));

            return new DiscordCdnUrl(DiscordCdnUrlType.StickerPackBanner, null, assetId,
                $"{CdnBaseUrl}/app-assets/710982414301790216/store/{assetId}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a team icon.
        /// </summary>
        /// <param name="teamId">The ID of the team.</param>
        /// <param name="iconHash">The hash of the team's icon.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="iconHash"/> is null.</exception>
        public static DiscordCdnUrl ForTeamIcon(Snowflake teamId, string iconHash)
        {
            if (iconHash == null) throw new ArgumentNullException(nameof(iconHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.TeamIcon, teamId, iconHash,
                $"{CdnBaseUrl}/team-icons/{teamId}/{iconHash}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a sticker.
        /// </summary>
        /// <param name="stickerId">The ID of the sticker.</param>
        public static DiscordCdnUrl ForSticker(Snowflake stickerId)
        {
            return new DiscordCdnUrl(DiscordCdnUrlType.Sticker, stickerId, stickerId.ToString(),
                $"{CdnBaseUrl}/stickers/{stickerId}");
        }

        /// <summary>
        /// Creates a CDN URL builder for a role icon.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="iconHash">The hash of the role's icon.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="iconHash"/> is null.</exception>
        public static DiscordCdnUrl ForRoleIcon(Snowflake roleId, string iconHash)
        {
            if (iconHash == null) throw new ArgumentNullException(nameof(iconHash));

            return new DiscordCdnUrl(DiscordCdnUrlType.RoleIcon, roleId, iconHash,
                $"{CdnBaseUrl}/role-icons/{roleId}/{iconHash}");
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
                return $"{CdnBaseUrl}/{BaseUrl}.{ext}?size={size.Value}";
            else
                return $"{CdnBaseUrl}/{BaseUrl}.{ext}";
        }

        public override string ToString()
        {
            return $"{CdnBaseUrl}/{BaseUrl}";
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
                   BaseUrl == other.BaseUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ResourceId, FileName, BaseUrl);
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
