using Discore.Audio;
using System;
using System.Runtime.ExceptionServices;

#pragma warning disable 1591

namespace Discore
{
    public class IntegrationEventArgs : EventArgs
    {
        public DiscordIntegration Integration { get; }

        public IntegrationEventArgs(DiscordIntegration integration)
        {
            Integration = integration;
        }
    }

    public class TypingStartEventArgs : EventArgs
    {
        public DiscordUser User { get; }
        public DiscordChannel Channel { get; }
        public long Timestamp { get; }

        public TypingStartEventArgs(DiscordUser user, DiscordChannel channel, long timestamp)
        {
            User = user;
            Channel = channel;
            Timestamp = timestamp;
        }
    }

    public class GuildMemberEventArgs : EventArgs
    {
        public DiscordGuild Guild { get; }
        public DiscordGuildMember Member { get; }

        public GuildMemberEventArgs(DiscordGuild guild, DiscordGuildMember member)
        {
            Guild = guild;
            Member = member;
        }
    }

    public class GuildUserEventArgs : EventArgs
    {
        public DiscordGuild Guild { get; }
        public DiscordUser User { get; }

        public GuildUserEventArgs(DiscordGuild guild, DiscordUser user)
        {
            Guild = guild;
            User = user;
        }
    }

    public class GuildRoleEventArgs : EventArgs
    {
        public DiscordGuild Guild { get; }
        public DiscordRole Role { get; }

        public GuildRoleEventArgs(DiscordGuild guild, DiscordRole role)
        {
            Guild = guild;
            Role = role;
        }
    }

    public class ChannelEventArgs : EventArgs
    {
        public DiscordChannel Channel { get; }

        public ChannelEventArgs(DiscordChannel channel)
        {
            Channel = channel;
        }
    }

    public class GuildEventArgs : EventArgs
    {
        public DiscordGuild Guild { get; }

        public GuildEventArgs(DiscordGuild guild)
        {
            Guild = guild;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public DiscordMessage Message { get; }

        public MessageEventArgs(DiscordMessage message)
        {
            Message = message;
        }
    }

    public class ExceptionDispathEventArgs : EventArgs
    {
        public ExceptionDispatchInfo Info { get; }

        public ExceptionDispathEventArgs(ExceptionDispatchInfo info)
        {
            Info = info;
        }
    }

    public class VoiceClientEventArgs : EventArgs
    {
        public DiscordVoiceClient VoiceClient { get; }

        public VoiceClientEventArgs(DiscordVoiceClient voiceClient)
        {
            VoiceClient = voiceClient;
        }
    }

    public class VoiceClientExceptionEventArgs : EventArgs
    {
        public DiscordVoiceClient VoiceClient { get; }
        public Exception Exception { get; }

        public VoiceClientExceptionEventArgs(DiscordVoiceClient voiceClient, Exception exception)
        {
            VoiceClient = voiceClient;
            Exception = exception;
        }
    }
}

#pragma warning restore 1591
