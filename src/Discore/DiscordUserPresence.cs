using System.Collections.Generic;
using System.Linq;

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
        public DiscordGame Game { get; }

        /// <summary>
        /// Gets the current status of this user.
        /// </summary>
        public DiscordUserStatus Status { get; }

        /// <summary>
        /// Gets the user's current activities.
        /// </summary>
        public IReadOnlyList<DiscordGame> Activities;

        /// <summary>
        /// Gets the user's platform-dependent status.
        /// </summary>
        public DiscordClientStatus ClientStatus { get; }

        internal DiscordUserPresence(Snowflake userId, DiscordApiData data)
        {
            UserId = userId;

            // Game
            DiscordApiData gameData = data.Get("game");
            if (gameData != null)
            {
                if (gameData.IsNull)
                    Game = null;
                else
                    Game = new DiscordGame(gameData);
            }

            // Status
            string statusStr = data.GetString("status");
            if (statusStr != null)
            {
                DiscordUserStatus? status = Utils.ParseUserStatus(statusStr);

                if (!status.HasValue)
                {
                    // If we don't have a value for the status yet, 
                    // we at least know that they aren't offline.
                    Status = DiscordUserStatus.Online;

                    // However, this should issue a warning.
                    DiscoreLogger.Global.LogWarning($"[DiscordUserPresence] Failed to deserialize status for user {UserId}. " +
                        $"status = {statusStr}");
                }
                else
                    Status = status.Value;
            }

            // Client status
            DiscordApiData clientStatusData = data.Get("client_status");
            if (clientStatusData != null)
                ClientStatus = new DiscordClientStatus(clientStatusData);

            // Activites
            IList<DiscordApiData> activitiesArray = data.GetArray("activities");
            if (activitiesArray != null)
                Activities = activitiesArray.Select(a => new DiscordGame(a)).ToArray();
        }
    }
}
