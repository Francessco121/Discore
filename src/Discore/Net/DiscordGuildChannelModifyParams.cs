namespace Discore.Net
{
    public class DiscordGuildChannelModifyParams
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public string Topic { get; set; }
        public int Bitrate { get; set; }
        public int UserLimit { get; set; }
        public DiscordGuildChannelType Type { get; set; }

        public DiscordGuildChannelModifyParams(DiscordGuildChannelType type)
        {
            Type = type;
        }

        public DiscordGuildChannelModifyParams(DiscordGuildChannel existingChannel)
        {
            Name = existingChannel.Name;
            Position = existingChannel.Position;
            Topic = existingChannel.Topic;
            Bitrate = existingChannel.Bitrate;
            UserLimit = existingChannel.UserLimit;
            Type = existingChannel.GuildChannelType;
        }

        public DiscordGuildChannelModifyParams(string name, int position, string topic)
        {
            Name = name;
            Position = position;
            Topic = topic;
            Type = DiscordGuildChannelType.Text;
        }

        public DiscordGuildChannelModifyParams(string name, int position, int bitrate, int userLimit)
        {
            Name = name;
            Position = position;
            Bitrate = bitrate;
            UserLimit = userLimit;
            Type = DiscordGuildChannelType.Voice;
        }
    }
}
