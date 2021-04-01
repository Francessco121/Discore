using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

#nullable enable

namespace Discore
{
    public sealed class DiscordUserPresence
    {
        /// <summary>
        /// Gets the ID of the user this presence is for.
        /// </summary>
        public Snowflake UserId { get; }

        /// <summary>
        /// Gets the game this user is currently playing.
        /// </summary>
        public DiscordGame? Game { get; }

        /// <summary>
        /// Gets the current status of this user.
        /// </summary>
        public DiscordUserStatus? Status { get; }

        /// <summary>
        /// Gets the user's current activities.
        /// </summary>
        public IReadOnlyList<DiscordGame>? Activities { get; }

        /// <summary>
        /// Gets the user's platform-dependent status.
        /// </summary>
        public DiscordClientStatus? ClientStatus { get; }

        public DiscordUserPresence(
            Snowflake userId, 
            DiscordGame? game, 
            DiscordUserStatus? status, 
            IReadOnlyList<DiscordGame>? activities, 
            DiscordClientStatus? clientStatus)
        {
            UserId = userId;
            Game = game;
            Status = status;
            Activities = activities;
            ClientStatus = clientStatus;
        }

        internal DiscordUserPresence(JsonElement json)
        {
            UserId = json.GetProperty("user").GetProperty("id").GetSnowflake();

            // Game
            JsonElement? gameJson = json.GetPropertyOrNull("game");
            Game = gameJson == null || gameJson.Value.ValueKind == JsonValueKind.Null
                ? null
                : new DiscordGame(gameJson.Value);

            // Status
            JsonElement? statusJson = json.GetPropertyOrNull("status");

            if (statusJson != null)
            {
                string? statusStr = statusJson.Value.GetString();

                if (statusStr != null)
                {
                    Status = Utils.ParseUserStatus(statusStr);

                    if (Status == null)
                    {
                        // If we don't have a value for the status yet, 
                        // we at least know that they aren't offline.
                        Status = DiscordUserStatus.Online;

                        // However, this should issue a warning.
                        DiscoreLogger.Global.LogWarning($"[DiscordUserPresence] Failed to deserialize status for user {UserId}. " +
                            $"status = {statusStr}");
                    }
                }
            }

            // Client status
            JsonElement? clientStatusJson = json.GetPropertyOrNull("client_status");
            ClientStatus = clientStatusJson == null || clientStatusJson.Value.ValueKind == JsonValueKind.Null
                ? null
                : new DiscordClientStatus(clientStatusJson.Value);

            // Activities
            JsonElement? activitiesJson = json.GetPropertyOrNull("activities");
            Activities = activitiesJson == null || activitiesJson.Value.ValueKind == JsonValueKind.Null
                ? null
                : activitiesJson.Value
                    .EnumerateArray()
                    .Select(a => new DiscordGame(a))
                    .ToArray();
        }
    }
}

#nullable restore
