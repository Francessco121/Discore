using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.WebSocket
{
    /// <summary>
    /// Represents a message sent in a channel within Discord.
    /// </summary>
    public sealed class DiscordMessage : DiscordIdObject
    {
        public const int MAX_CHARACTERS = 2000;

        /// <summary>
        /// Gets the channel this message is in.
        /// </summary>
        public DiscordChannel Channel { get; private set; }
        /// <summary>
        /// Gets the author of this message.
        /// </summary>
        public DiscordUser Author { get; private set; }
        /// <summary>
        /// Gets the guild this message was sent in (or null if a DM message).
        /// </summary>
        public DiscordGuild Guild { get; private set; }
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
        public DiscordApiCacheIdSet<DiscordUser> Mentions { get; }
        /// <summary>
        /// Gets a list of all mentioned roles in this message.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordRole> MentionedRoles { get; }
        /// <summary>
        /// Gets a table of all attachments in this message.
        /// </summary>
        public DiscordApiCacheTable<DiscordAttachment> Attachments { get; }
        /// <summary>
        /// Gets a list of all embedded attachments in this message.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds { get; private set; }
        /// <summary>
        /// Gets a list of all reactions to this message.
        /// </summary>
        public IReadOnlyList<DiscordReaction> Reactions { get; private set; }
        /// <summary>
        /// Used for validating if a message was sent.
        /// </summary>
        public Snowflake? Nonce { get; private set; }
        /// <summary>
        /// Gets whether or not this message is pinned in the containing channel.
        /// </summary>
        public bool IsPinned { get; private set; }

        Shard shard;
        HttpChannelsEndpoint channelsHttp;

        internal DiscordMessage(Shard shard)
        {
            this.shard = shard;
            channelsHttp = shard.Application.InternalHttpApi.Channels;

            Mentions = new DiscordApiCacheIdSet<DiscordUser>(shard.Users);
            MentionedRoles = new DiscordApiCacheIdSet<DiscordRole>(shard.Roles);
            Attachments = new DiscordApiCacheTable<DiscordAttachment>();
        }

        public bool AddReaction(string emojiName)
        {
            DiscordApiData data = channelsHttp.CreateReaction(Channel.Id, Id, emojiName);
            return data.IsNull;
        }

        public bool AddReaction(DiscordReactionEmoji emoji)
        {
            if (emoji.Id.HasValue)
                return AddReaction(emoji.Name, emoji.Id.Value);
            else
                return AddReaction(emoji.Name);
        }

        public bool AddReaction(string customEmojiName, Snowflake customEmojiId)
        {
            DiscordApiData data = channelsHttp.CreateReaction(Channel.Id, Id, customEmojiName, customEmojiId);
            return data.IsNull;
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

        /// <summary>
        /// Updates an existing <see cref="DiscordMessage"/> object with data
        /// received from a MessageUpdated event.
        /// </summary>
        public static void UpdateMessage(DiscordMessage existingMessage, DiscordApiData updates)
        {
            existingMessage.Update(updates);
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Content         = data.GetString("content") ?? Content;
            Timestamp       = data.GetDateTime("timestamp") ?? Timestamp;
            EditedTimestamp = data.GetDateTime("edited_timestamp") ?? EditedTimestamp;
            TextToSpeech    = data.GetBoolean("tts") ?? TextToSpeech;
            MentionEveryone = data.GetBoolean("mention_everyone") ?? MentionEveryone;
            Nonce           = data.GetSnowflake("nonce") ?? Nonce;
            IsPinned        = data.GetBoolean("pinned") ?? IsPinned;

            Snowflake channelId = data.GetSnowflake("channel_id").Value;
            Channel = shard.Channels.Get(channelId);

            DiscordGuildChannel guildChannel = Channel as DiscordGuildChannel;
            if (guildChannel != null)
                // Message was sent in a guild
                Guild = guildChannel.Guild;
            else
                Guild = null;

            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                Snowflake authorId = authorData.GetSnowflake("id").Value;
                Author = shard.Users.Edit(authorId, () => new DiscordUser(), user => user.Update(authorData));
            }

            IList<DiscordApiData> mentionsData = data.GetArray("mentions");
            if (mentionsData != null)
            {
                Mentions.Clear();
                for (int i = 0; i < mentionsData.Count; i++)
                {
                    DiscordApiData mentionData = mentionsData[i];
                    Snowflake mentionedUserId = mentionData.GetSnowflake("id").Value;

                    // Follow through with the "eventual consistency" and update the user,
                    // but only save the id on our end.
                    shard.Users.Edit(mentionedUserId, () => new DiscordUser(), user => user.Update(mentionData));

                    Mentions.Add(mentionedUserId);
                }
            }

            IList<DiscordApiData> mentionedRolesArray = data.GetArray("mention_roles");
            if (mentionedRolesArray != null)
            {
                MentionedRoles.Clear();
                for (int i = 0; i < mentionedRolesArray.Count; i++)
                {
                    Snowflake roleId = mentionedRolesArray[i].ToSnowflake().Value;
                    MentionedRoles.Add(roleId);
                }
            }

            IList<DiscordApiData> attachmentsData = data.GetArray("attachments");
            if (attachmentsData != null)
            {
                Attachments.Clear();
                for (int i = 0; i < attachmentsData.Count; i++)
                {
                    DiscordApiData attachmentData = attachmentsData[i];
                    Snowflake attachmentId = attachmentData.GetSnowflake("id").Value;

                    Attachments.Edit(attachmentId, () => new DiscordAttachment(), at => at.Update(attachmentData));
                }
            }

            IList<DiscordApiData> embedsArray = data.GetArray("embeds");
            if (embedsArray != null)
            {
                DiscordEmbed[] embeds = new DiscordEmbed[embedsArray.Count];
                for (int i = 0; i < embeds.Length; i++)
                {
                    DiscordEmbed embed = new DiscordEmbed();
                    embed.Update(embedsArray[i]);
                    embeds[i] = embed;
                }

                Embeds = new ReadOnlyCollection<DiscordEmbed>(embeds);
            }

            IList<DiscordApiData> reactionsArray = data.GetArray("reactions");
            if (reactionsArray != null)
            {
                DiscordReaction[] reactions = new DiscordReaction[reactionsArray.Count];
                for (int i = 0; i < reactions.Length; i++)
                {
                    DiscordReaction reaction = new DiscordReaction();
                    reaction.Update(reactionsArray[i]);
                    reactions[i] = reaction;
                }

                Reactions = new ReadOnlyCollection<DiscordReaction>(reactions);
            }
        }
    }
}
