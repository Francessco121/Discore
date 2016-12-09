using System;

namespace Discore
{
    public sealed class DiscordUserPresence
    {
        /// <summary>
        /// Gets the id of the user this presence is for.
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

        internal DiscordUserPresence(DiscordApiData data, Snowflake userId)
        {
            UserId = userId;

            DiscordApiData gameData = data.Get("game");
            if (gameData != null)
            {
                if (gameData.IsNull)
                    Game = null;
                else
                    Game = new DiscordGame(gameData);
            }

            string statusStr = data.GetString("status");
            if (statusStr != null)
            {
                DiscordUserStatus status;
                if (Enum.TryParse(statusStr, true, out status))
                    Status = status;
            }
        }
    }
}
