using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// Represents a message sent in a channel within Discord.
    /// </summary>
    public class DiscordMessage : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this message.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the channel this message is in.
        /// </summary>
        public DiscordChannel Channel { get; private set; }
        /// <summary>
        /// Gets the author of this message.
        /// </summary>
        public DiscordUser Author { get; private set; }
        /// <summary>
        /// Gets the guild member instance of the author of this message
        /// if it was sent in a <see cref="DiscordGuildChannel"/>.
        /// </summary>
        public DiscordGuildMember AuthorMember { get; private set; }
        /// <summary>
        /// Gets the contents of this message.
        /// </summary>
        public string Content { get; private set; }
        /// <summary>
        /// Gets the time this message was first sent.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>
        /// Gets the time of the last edit to this message.
        /// </summary>
        public DateTime? EditedTimestamp { get; private set; }
        /// <summary>
        /// Gets whether or not this message was sent with the /tts command.
        /// </summary>
        public bool TextToSpeech { get; private set; }
        /// <summary>
        /// Gets whether or not this message mentioned everyone via @everyone.
        /// </summary>
        public bool MentionEveryone { get; private set; }
        /// <summary>
        /// Gets a list of all user-specific mentions in this message.
        /// </summary>
        public DiscordUser[] Mentions { get; private set; }
        /// <summary>
        /// Gets a list of all attachments in this message.
        /// </summary>
        public DiscordAttachment[] Attachments { get; private set; }
        /// <summary>
        /// Gets a list of all embedded attachments in this message.
        /// </summary>
        public DiscordEmbed[] Embeds { get; private set; }
        /// <summary>
        /// Used for validating if a message was sent.
        /// </summary>
        public string Nonce { get; private set; }
        /// <summary>
        /// Gets whether or not this message is pinned in the containing channel.
        /// </summary>
        public bool IsPinned { get; private set; }

        IDiscordClient client;
        DiscordApiCache cache;

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="client"></param>
        public DiscordMessage(IDiscordClient client)
        {
            this.client = client;
            cache = client.Cache;
        }

        /// <summary>
        /// Changes the contents of this message.
        /// </summary>
        /// <param name="newContent">The new contents.</param>
        public async Task<DiscordMessage> Edit(string newContent)
        {
            Content = newContent;
            return await client.Rest.Messages.Edit(Channel, Id, newContent);
        }

        /// <summary>
        /// Deletes this message.
        /// </summary>
        public async Task<bool> Delete()
        {
            return await client.Rest.Messages.Delete(Channel, Id);
        }

        /// <summary>
        /// Updates this message with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this message with.</param>
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

            IList<DiscordApiData> mentionsData = data.GetArray("mentions");
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

            IList<DiscordApiData> attachmentsJson = data.GetArray("attachments");
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

            IList<DiscordApiData> embedsData = data.GetArray("embeds");
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

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordMessage"/> is equal 
        /// to the current message.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordMessage"/> to check.</param>
        public bool Equals(DiscordMessage other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current message.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordMessage other = obj as DiscordMessage;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this message.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the id of this message.
        /// </summary>
        public override string ToString()
        {
            return Id;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordMessage a, DiscordMessage b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordMessage a, DiscordMessage b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
