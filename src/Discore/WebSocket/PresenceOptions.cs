using System.Collections.Generic;

namespace Discore.WebSocket
{
    public class PresenceOptions
    {
        /// <summary>
        /// The status of the bot. Defaults to <see cref="DiscordUserStatus.Online"/>.
        /// </summary>
        public DiscordUserStatus Status { get; set; } = DiscordUserStatus.Online;

        /// <summary>
        /// Whether the bot is AFK. Defaults to false.
        /// </summary>
        public bool Afk { get; set; } = false;

        /// <summary>
        /// Unix time (in milliseconds) of when the bot went idle,
        /// or null if the bot is not idle. Defaults to null.
        /// </summary>
        public int? AfkSince { get; set; }

        /// <summary>
        /// Each activity the bot is taking part in. If none are specified, then
        /// the bot will not be shown as doing anything.
        /// Defaults to null.
        /// </summary>
        /// <remarks>
        /// Usually, there will only be zero or one activity.
        /// </remarks>
        public IList<ActivityOptions>? Activities { get; set; }

        public PresenceOptions() { }

        /// <summary>
        /// Sets the status of the bot.
        /// </summary>
        public PresenceOptions SetStatus(DiscordUserStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Sets whether the bot is AFK.
        /// </summary>
        public PresenceOptions SetAfk(bool afk)
        {
            Afk = afk;
            return this;
        }

        /// <summary>
        /// Sets the unix time (in milliseconds) of when the bot went idle,
        /// or null if the bot is not idle.
        /// </summary>
        public PresenceOptions SetAfkSince(int? afkSince)
        {
            AfkSince = afkSince;
            return this;
        }

        /// <summary>
        /// Sets each activity the bot is taking part in. If none are specified, then
        /// the bot will not be shown as doing anything.
        /// </summary>
        public PresenceOptions SetActivities(IList<ActivityOptions> activities)
        {
            Activities = activities;
            return this;
        }

        /// <summary>
        /// Adds an activity that the bot is taking part in.
        /// </summary>
        public PresenceOptions AddActivity(ActivityOptions activity)
        {
            Activities ??= new List<ActivityOptions>();
            Activities.Add(activity);
            return this;
        }
    }
}
