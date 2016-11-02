using Discore.Net.Rest;
using System;
using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// Represents a message sent in a channel within Discord.
    /// </summary>
    public class DiscordMessage : DiscordIdObject
    {
        /// <summary>
        /// Gets the channel this message is in.
        /// </summary>
        public DiscordChannel Channel { get { return cache.Get<DiscordChannel>(channelId); } }
        /// <summary>
        /// Gets the author of this message.
        /// </summary>
        public DiscordUser Author { get { return cache.Get<DiscordUser>(authorId); } }
        ///// <summary>
        ///// Gets the guild member instance of the author of this message
        ///// if it was sent in a <see cref="DiscordGuildChannel"/>.
        ///// </summary>
        //public DiscordGuildMember AuthorMember
        //{
        //    get
        //    {
        //        // todo
        //    }
        //}
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
        public DiscordUser[] Mentions { get { return cache.Get<DiscordUser>(mentionIds); } }
        /// <summary>
        /// Gets a list of all attachments in this message.
        /// </summary>
        public DiscordAttachment[] Attachments { get { return cache.Get<DiscordAttachment>(attachmentIds); } }
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

        string channelId;
        string authorId;
        string[] mentionIds;
        string[] attachmentIds;

        DiscordApiCache cache;
        DiscordRestApi rest;

        internal DiscordMessage(Shard shard)
        {
            cache = shard.Cache;
            rest = shard.Application.Rest;
        }

        ///// <summary>
        ///// Changes the contents of this message.
        ///// </summary>
        ///// <param name="newContent">The new contents.</param>
        //public DiscordMessage Edit(string newContent)
        //{
        //    Content = newContent;
        //    return rest.Messages.Edit(Channel, Id, newContent);
        //}

        ///// <summary>
        ///// Deletes this message.
        ///// </summary>
        //public bool Delete()
        //{
        //    return rest.Messages.Delete(Channel, Id);
        //}

        ///// <summary>
        ///// Pins this message.
        ///// </summary>
        //public bool Pin()
        //{
        //    return rest.Messages.Pin(this);
        //}

        internal override void Update(DiscordApiData data)
        {
            Id              = data.GetString("id") ?? Id;
            Content         = data.GetString("content") ?? Content;
            Timestamp       = data.GetDateTime("timestamp") ?? Timestamp;
            EditedTimestamp = data.GetDateTime("edited_timestamp") ?? EditedTimestamp;
            TextToSpeech    = data.GetBoolean("tts") ?? TextToSpeech;
            MentionEveryone = data.GetBoolean("mention_everyone") ?? MentionEveryone;
            Nonce           = data.GetString("nonce") ?? Nonce;
            IsPinned        = data.GetBoolean("pinned") ?? IsPinned;
            channelId       = data.GetString("channel_id") ?? channelId;

            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                authorId = authorData.GetString("id");
                cache.Set(authorData, authorId, () => new DiscordUser());
            }

            IList<DiscordApiData> mentionsData = data.GetArray("mentions");
            if (mentionsData != null)
            {
                mentionIds = new string[mentionsData.Count];
                for (int i = 0; i < mentionIds.Length; i++)
                {
                    DiscordApiData mentionData = mentionsData[i];
                    string mentionedUserId = mentionData.GetString("id");

                    mentionIds[i] = mentionedUserId;
                    cache.Set(mentionData, mentionedUserId, () => new DiscordUser());
                }
            }

            IList<DiscordApiData> attachmentsData = data.GetArray("attachments");
            if (attachmentsData != null)
            {
                attachmentIds = new string[attachmentsData.Count];
                for (int i = 0; i < attachmentIds.Length; i++)
                {
                    DiscordApiData attachmentData = attachmentsData[i];
                    string attachmentId = attachmentData.GetString("id");

                    attachmentIds[i] = attachmentId;
                    cache.Set(attachmentData, attachmentId, () => new DiscordAttachment());
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
    }
}
