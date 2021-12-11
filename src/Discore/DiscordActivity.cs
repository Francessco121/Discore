using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Represents an activity a user is currently engaged in.
    /// </summary>
    public class DiscordActivity
    {
        /// <summary>
        /// Gets the name of the activity.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of activity.
        /// </summary>
        public DiscordActivityType Type { get; }
        /// <summary>
        /// Gets the URL of the stream when the type is set to "Streaming" and the URL is valid.
        /// Otherwise, returns null.
        /// </summary>
        public string? Url { get; }
        /// <summary>
        /// Gets the unix timestamp (in milliseconds) of when the activity was added to the user's session.
        /// </summary>
        public long? CreatedAt { get; }
        /// <summary>
        /// Gets the unix timestamps for start and/or end of the game. May be null.
        /// </summary>
        public DiscordActivityTimestamps? Timestamps { get; }
        /// <summary>
        /// Gets the application ID for the game.
        /// </summary>
        public Snowflake? ApplicationId { get; }
        /// <summary>
        /// Gets what the player is currently doing. May be null.
        /// </summary>
        public string? Details { get; }
        /// <summary>
        /// Gets the user's current party status. May be null.
        /// </summary>
        public string? State { get; }
        /// <summary>
        /// Gets the emoji used for a custom status. May be null.
        /// </summary>
        public DiscordActivityEmoji? Emoji { get; }
        /// <summary>
        /// Gets information for the current party of the player. May be null.
        /// </summary>
        public DiscordActivityParty? Party { get; }
        /// <summary>
        /// Gets images for the presence and their hover texts. May be null.
        /// </summary>
        public DiscordActivityAssets? Assets { get; }
        /// <summary>
        /// Gets secrets for Rich Presence joining and spectating. May be null.
        /// </summary>
        public DiscordActivitySecrets? Secrets { get; }
        /// <summary>
        /// Gets whether the activity is an instanced game session.
        /// </summary>
        public bool? Instance { get; }
        /// <summary>
        /// Gets flags describing what the payload includes.
        /// </summary>
        public DiscordActivityFlag Flags { get; }
        /// <summary>
        /// Gets a list of custom button labels shown in the Rich Presence. May be null.
        /// </summary>
        public IReadOnlyList<string>? Buttons { get; }

        internal DiscordActivity(JsonElement json)
        {
            Name = json.GetProperty("name").GetString()!;
            Type = (DiscordActivityType)json.GetProperty("type").GetInt32();
            Url = json.GetPropertyOrNull("url")?.GetString();
            CreatedAt = json.GetProperty("created_at").GetInt64();
            ApplicationId = json.GetPropertyOrNull("application_id")?.GetSnowflake();
            Details = json.GetPropertyOrNull("details")?.GetString();
            State = json.GetPropertyOrNull("state")?.GetString();
            Instance = json.GetPropertyOrNull("instance")?.GetBoolean();
            Flags = (DiscordActivityFlag)(json.GetPropertyOrNull("flags")?.GetInt32() ?? 0);

            JsonElement? timestampsJson = json.GetPropertyOrNull("timestamps");
            if (timestampsJson != null)
                Timestamps = new DiscordActivityTimestamps(timestampsJson.Value);

            JsonElement? emojiJson = json.GetPropertyOrNull("emoji");
            if (emojiJson != null && emojiJson.Value.ValueKind != JsonValueKind.Null)
                Emoji = new DiscordActivityEmoji(emojiJson.Value);

            JsonElement? partyJson = json.GetPropertyOrNull("party");
            if (partyJson != null)
                Party = new DiscordActivityParty(partyJson.Value);

            JsonElement? assetsJson = json.GetPropertyOrNull("assets");
            if (assetsJson != null)
                Assets = new DiscordActivityAssets(assetsJson.Value);

            JsonElement? secretsJson = json.GetPropertyOrNull("secrets");
            if (secretsJson != null)
                Secrets = new DiscordActivitySecrets(secretsJson.Value);

            JsonElement? buttonsJson = json.GetPropertyOrNull("buttons");
            if (buttonsJson != null)
            {
                JsonElement _buttonsJson = buttonsJson.Value;
                string[] buttons = new string[_buttonsJson.GetArrayLength()];

                for (int i = 0; i < buttons.Length; i++)
                    buttons[i] = _buttonsJson[i].GetString()!;

                Buttons = buttons;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
