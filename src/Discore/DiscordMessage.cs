using System;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordMessage : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public DiscordChannel Channel { get; private set; }
        public DiscordUser Author { get; private set; }
        public DiscordGuildMember AuthorMember { get; private set; }
        public string Content { get; private set; }
        public DateTime Timestamp { get; private set; }
        public DateTime? EditedTimestamp { get; private set; }
        public bool TextToSpeech { get; private set; }
        public bool MentionEveryone { get; private set; }
        public DiscordUser[] Mentions { get; private set; }
        public DiscordAttachment[] Attachments { get; private set; }
        public DiscordEmbed[] Embeds { get; private set; }
        public string Nonce { get; private set; }
        public bool IsPinned { get; private set; }

        IDiscordClient client;
        DiscordApiCache cache;

        public DiscordMessage(IDiscordClient client)
        {
            this.client = client;
            cache = client.Cache;
        }

        public void Edit(string newContent)
        {
            Content = newContent;
            client.Rest.Messages.Edit(Channel, Id, newContent);
        }

        public void Update(DiscordApiData data)
        {
            Id              = data.GetString("id") ?? Id;
            Content         = data.GetString("content") ?? Content;
            Timestamp       = data.GetDateTime("timestamp") ?? Timestamp;
            EditedTimestamp = data.GetDateTime("edited_timestamp") ?? EditedTimestamp;
            TextToSpeech    = data.GetBoolean("tts") ?? TextToSpeech;
            MentionEveryone = data.GetBoolean("mention_everyone") ?? MentionEveryone;
            Nonce           = data.GetString("nonce") ?? Nonce;
            IsPinned        = data.GetBoolean("pinned") ?? IsPinned;

            string channelId = data.GetString("channel_id");
            if (channelId != null)
            {
                DiscordChannel channel;
                if (cache.TryGet(channelId, out channel))
                    Channel = channel;
                else
                    DiscordLogger.Default.LogWarning($"[MESSAGE.UPDATE] Failed to find channel with id {channelId}");
            }

            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                string authorId = authorData.GetString("id");
                Author = cache.AddOrUpdate(authorId, authorData, () => { return new DiscordUser(); });

                DiscordGuildChannel guildChannel = Channel as DiscordGuildChannel;
                if (guildChannel != null)
                {
                    DiscordGuildMember authorMember;
                    if (cache.TryGet(guildChannel.Guild, authorId, out authorMember))
                        AuthorMember = authorMember;
                }
            }

            IReadOnlyList<DiscordApiData> mentionsData = data.GetArray("mentions");
            if (mentionsData != null)
            {
                Mentions = new DiscordUser[mentionsData.Count];
                for (int i = 0; i < Mentions.Length; i++)
                {
                    string mentionedUserId = mentionsData[i].GetString("id");
                    DiscordUser mention;
                    if (cache.TryGet(mentionedUserId, out mention))
                        Mentions[i] = mention;
                }
            }

            IReadOnlyList<DiscordApiData> attachmentsJson = data.GetArray("attachments");
            if (attachmentsJson != null)
            {
                Attachments = new DiscordAttachment[attachmentsJson.Count];
                for (int i = 0; i < Attachments.Length; i++)
                {
                    DiscordAttachment attachment = new DiscordAttachment();
                    attachment.Update(attachmentsJson[i]);
                    Attachments[i] = attachment;
                }
            }

            IReadOnlyList<DiscordApiData> embedsData = data.GetArray("embeds");
            if (embedsData != null)
            {
                Embeds = new DiscordEmbed[embedsData.Count];
                for (int i = 0; i < Embeds.Length; i++)
                {
                    DiscordEmbed embed = new DiscordEmbed();
                    embed.Update(embedsData[i]);
                    Embeds[i] = embed;
                }
            }
        }

        public void Delete()
        {
            client.Rest.Messages.Delete(Channel, Id);
        }
    }
}
