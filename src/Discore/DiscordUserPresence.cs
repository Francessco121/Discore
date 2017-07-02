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

        internal DiscordUserPresence(Snowflake userId, DiscordApiData data)
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
                DiscordUserStatus? status = ParseStatus(statusStr);

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
        }

        DiscordUserStatus? ParseStatus(string str)
        {
            switch (str)
            {
                case "offline":
                    return DiscordUserStatus.Offline;
                case "dnd":
                    return DiscordUserStatus.DoNotDisturb;
                case "idle":
                    return DiscordUserStatus.Idle;
                case "online":
                    return DiscordUserStatus.Online;
                default:
                    return null;
            }
        }
    }
}
