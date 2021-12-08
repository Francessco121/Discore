using System.Collections.Generic;
using System.Linq;

namespace Discore
{
    /// <summary>
    /// Representation of the game a user is currently playing (aka. Activity).
    /// </summary>
    public sealed class DiscordGame
    {
        /// <summary>
        /// Gets the name of the activity.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of the activity.
        /// </summary>
        public DiscordGameType Type { get; }
        /// <summary>
        /// Gets the URL of the stream when the type is set to "Streaming" and the URL is valid.
        /// Otherwise, returns null.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Gets the unix timestamp (in milliseconds) of when the activity was added to the user's session.
        /// </summary>
        public long? CreatedAt { get; }
        /// <summary>
        /// Gets the unix timestamps for start and/or end of the game. May be null.
        /// </summary>
        public DiscordActivityTimestamps Timestamps { get; }
        /// <summary>
        /// Gets the application ID for the game.
        /// </summary>
        public Snowflake? ApplicationId { get; }
        /// <summary>
        /// Gets what the player is currently doing. May be null.
        /// </summary>
        public string Details { get; }
        /// <summary>
        /// Gets the user's current party status. May be null.
        /// </summary>
        public string State { get; }
        /// <summary>
        /// Gets the emoji used for a custom status. May be null.
        /// </summary>
        public DiscordActivityEmoji Emoji { get; }
        /// <summary>
        /// Gets information for the current party of the player. May be null.
        /// </summary>
        public DiscordActivityParty Party { get; }
        /// <summary>
        /// Gets images for the presence and their hover texts. May be null.
        /// </summary>
        public DiscordActivityAssets Assets { get; }
        /// <summary>
        /// Gets secrets for Rich Presence joining and spectating. May be null.
        /// </summary>
        public DiscordActivitySecrets Secrets { get; }
        /// <summary>
        /// Gets whether the activity is an instanced game session.
        /// </summary>
        public bool? Instance { get; }
        /// <summary>
        /// Gets flags describing what the payload includes.
        /// </summary>
        public DiscordActivityFlag Flags { get; }
        /// <summary>
        /// Gets a list of custom button labels shown in the Rich Presence.
        /// </summary>
        public IReadOnlyList<string> Buttons { get; }

        internal DiscordGame(DiscordApiData data)
        {
            Name = data.GetString("name");
            Type = (DiscordGameType)(data.GetInteger("type") ?? 0);
            Url = data.GetString("url");
            CreatedAt = data.GetInt64("created_at");
            ApplicationId = data.GetSnowflake("application_id");
            Details = data.GetString("details");
            State = data.GetString("state");
            Instance = data.GetBoolean("instance");
            Flags = (DiscordActivityFlag)(data.GetInteger("flags") ?? 0);

            DiscordApiData timestampsData = data.Get("timestamps");
            if (timestampsData != null)
                Timestamps = new DiscordActivityTimestamps(timestampsData);

            DiscordApiData emojiData = data.Get("emoji");
            if (emojiData != null)
                Emoji = new DiscordActivityEmoji(emojiData);

            DiscordApiData partyData = data.Get("party");
            if (partyData != null)
                Party = new DiscordActivityParty(partyData);

            DiscordApiData assetsData = data.Get("assets");
            if (assetsData != null)
                Assets = new DiscordActivityAssets(assetsData);

            DiscordApiData secretsData = data.Get("secrets");
            if (secretsData != null)
                Secrets = new DiscordActivitySecrets(secretsData);

            IList<DiscordApiData> buttonsArray = data.GetArray("buttons");
            if (buttonsArray != null)
                Buttons = buttonsArray.Select(d => d.ToString()).ToArray();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
