using Discore.Audio;
using System;
using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// An exception thrown by a <see cref="DiscordApiCacheHelper"/> instance.
    /// </summary>
    public class DiscordApiCacheHelperException : DiscoreException
    {
        /// <summary>
        /// Creates a new <see cref="DiscordApiCacheHelperException"/> instance.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public DiscordApiCacheHelperException(string message) 
            : base(message)
        { }
    }

    /// <summary>
    /// A helper class for interacting with a <see cref="DiscordApiCache"/>.
    /// </summary>
    public class DiscordApiCacheHelper
    {
        DiscordApiCache cache;
        IDiscordClient client;

        /// <summary>
        /// Creates a new <see cref="DiscordApiCacheHelper"/> instance.
        /// </summary>
        /// <param name="client">The <see cref="IDiscordClient"/> this <see cref="DiscordApiCacheHelper"/> 
        /// is working with.</param>
        /// <param name="cache">The <see cref="DiscordApiCache"/> this <see cref="DiscordApiCacheHelper"/>
        /// is working with.</param>
        public DiscordApiCacheHelper(IDiscordClient client, DiscordApiCache cache)
        {
            this.client = client;
            this.cache = cache;
        }

        #region Channel Events
        /// <summary>
        /// Creates a <see cref="DiscordChannel"/> from the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> to use to create the <see cref="DiscordChannel"/>.</param>
        /// <returns>Returns the created <see cref="DiscordChannel"/>.</returns>
        public DiscordChannel CreateChannel(DiscordApiData data)
        {
            if (data.ContainsKey("recipient"))
            {
                // DM
                string channelId = data.GetString("id");
                DiscordDMChannel dmChannel = cache.AddOrUpdate(channelId, data, 
                    () => { return new DiscordDMChannel(client); });
                cache.SetAlias<DiscordChannel>(dmChannel);

                return dmChannel;
            }
            else
            {
                // Guild
                string guildId = data.GetString("guild_id");
                DiscordGuild guild;
                if (cache.TryGet(guildId, out guild))
                {
                    string channelId = data.GetString("id");
                    DiscordGuildChannel channel = cache.AddOrUpdate(guild, channelId, data,
                        () => { return new DiscordGuildChannel(client, guild); });
                    cache.SetAlias<DiscordChannel>(channel);

                    return channel;
                }
                else
                    throw new DiscordApiCacheHelperException($"Received channel '{data.GetString("name")}' for unknown guild id '{guildId}'");
            }
        }

        /// <summary>
        /// Updates a <see cref="DiscordGuildChannel"/> with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> to update the <see cref="DiscordGuildChannel"/> with.</param>
        /// <returns>Returns the updated <see cref="DiscordGuildChannel"/>.</returns>
        public DiscordGuildChannel UpdateChannel(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                string channelId = data.GetString("id");
                DiscordGuildChannel updatedChannel = cache.AddOrUpdate(guild, channelId, data,
                    () => { return new DiscordGuildChannel(client, guild); });

                return updatedChannel;
            }
            else
                throw new DiscordApiCacheHelperException($"Received channel '{data.GetString("name")}' for unknown guild id '{guildId}'");
        }

        /// <summary>
        /// Deletes the <see cref="DiscordChannel"/> specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> describing the deletion.</param>
        /// <returns>Returns the deleted <see cref="DiscordChannel"/>.</returns>
        public DiscordChannel DeleteChannel(DiscordApiData data)
        {
            if (data.ContainsKey("recipient"))
            {
                // DM
                string dmChannelId = data.GetString("id");
                DiscordDMChannel deletedDMChannel;
                if (cache.TryRemove(dmChannelId, out deletedDMChannel))
                {
                    // Delete alias
                    cache.TryRemoveAlias<DiscordChannel>(dmChannelId);

                    return deletedDMChannel;
                }
                else
                    throw new DiscordApiCacheHelperException($"Received dm channel delete for unknown channel with id {dmChannelId}");
            }
            else
            {
                // Guild
                string guildId = data.GetString("guild_id");
                DiscordGuild guild;
                if (cache.TryGet(guildId, out guild))
                {
                    string deletedChannelId = data.GetString("id");
                    DiscordGuildChannel deletedChannel;
                    if (cache.TryRemove(guild, deletedChannelId, out deletedChannel))
                        return deletedChannel;
                    else
                        return null;
                }
                else
                    throw new DiscordApiCacheHelperException($"Attempted to delete channel '{data.GetString("name")}' for unknown guild id '{guildId}'");
            }
        }
        #endregion

        #region Guild Events
        /// <summary>
        /// Creates a <see cref="DiscordGuild"/> from the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> to use to create the <see cref="DiscordGuild"/>.</param>
        /// <returns>Returns the created <see cref="DiscordGuild"/>.</returns>
        public DiscordGuild CreateGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild createdGuild = cache.AddOrUpdate(guildId, data, () => { return new DiscordGuild(client); });

            return createdGuild;
        }

        /// <summary>
        /// Updates a <see cref="DiscordGuild"/> with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> to update the <see cref="DiscordGuild"/> with.</param>
        /// <returns>Returns the updated <see cref="DiscordGuild"/>.</returns>
        public DiscordGuild UpdateGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild updatedGuild = cache.AddOrUpdate(guildId, data, () => { return new DiscordGuild(client); });

            return updatedGuild;
        }

        /// <summary>
        /// Deletes the <see cref="DiscordGuild"/> specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> describing the deletion.</param>
        /// <returns>Returns the deleted <see cref="DiscordGuild"/>.</returns>
        public DiscordGuild DeleteGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild deletedGuild;
            if (cache.TryRemove(guildId, out deletedGuild))
                return deletedGuild;
            else
                throw new DiscordApiCacheHelperException($"Received delete for non-existant guild id {guildId}");
        }

        /// <summary>
        /// Adds a guild ban specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the add.</param>
        /// <returns>Returns the <see cref="DiscordGuild"/> and <see cref="DiscordUser"/> involved in the ban.</returns>
        public Tuple<DiscordGuild, DiscordUser> AddGuildBan(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.GetAndTryUpdate(guildId, data, out guild))
            {
                string userId = data.GetString("id");
                DiscordUser user = cache.AddOrUpdate(userId, data, () => { return new DiscordUser(); });

                return new Tuple<DiscordGuild, DiscordUser>(guild, user);
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild ban for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Removes a guild ban specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> describing the ban removal.</param>
        /// <returns>Returns the <see cref="DiscordGuild"/> and <see cref="DiscordUser"/> involved in the ban.</returns>
        public Tuple<DiscordGuild, DiscordUser> RemoveGuildBan(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.GetAndTryUpdate(guildId, data, out guild))
            {
                string userId = data.GetString("id");
                DiscordUser user = cache.AddOrUpdate(userId, data, () => { return new DiscordUser(); });

                return new Tuple<DiscordGuild, DiscordUser>(guild, user);
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild ban remove for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Updates a <see cref="DiscordGuild"/>'s emojis based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the <see cref="DiscordGuild"/> that had it's emojis updated.</returns>
        public DiscordGuild UpdateEmoji(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.GetAndTryUpdate(guildId, data, out guild))
            {
                // Standard update is fine here because the data
                // looks the same as a normal guild payload.
                guild.Update(data);

                return guild;
            }
            else
                throw new DiscordApiCacheHelperException($"Received emoji update for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Updates a <see cref="DiscordIntegration"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the <see cref="DiscordIntegration"/> updated.</returns>
        public DiscordIntegration UpdateGuildIntegrations(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.GetAndTryUpdate(guildId, data, out guild))
            {
                string integrationId = data.GetString("id");
                DiscordIntegration integration = cache.AddOrUpdate(guild, integrationId, data, 
                    () => { return new DiscordIntegration(client, guild); });

                return integration;
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild integrations update for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Adds a <see cref="DiscordGuildMember"/> based on the specified <see cref="DiscordApiData"/>. 
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the add.</param>
        /// <returns>Returns the added <see cref="DiscordGuildMember"/>.</returns>
        public DiscordGuildMember AddGuildMember(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");

            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                string memberId = data.LocateString("user.id");
                DiscordGuildMember member = cache.AddOrUpdate(guild, memberId, data,
                    () => { return new DiscordGuildMember(client, guild); }, true);

                return member;
            }
            else
                throw new DiscordApiCacheHelperException($"Received add for member '{data.LocateString("user.username")}' to "
                    + $"non-existant guild with id '{guildId}'");
        }

        /// <summary>
        /// Updates a <see cref="DiscordGuildMember"/> from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the updated <see cref="DiscordGuildMember"/>.</returns>
        public DiscordGuildMember UpdateGuildMember(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                string userId = data.LocateString("user.id");
                DiscordGuildMember member;
                if (cache.GetAndTryUpdate(guild, userId, data, out member))
                    return member;
                else
                    throw new DiscordApiCacheHelperException($"Received guild member update for unknown user with id {userId} "
                        + $"in guild '{guild.Name}'");
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild member update for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Removes the <see cref="DiscordGuildMember"/> specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the removal.</param>
        /// <returns>Returns the removed <see cref="DiscordGuildMember"/>.</returns>
        public DiscordGuildMember RemoveGuildMember(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                string userId = data.LocateString("user.id");
                DiscordGuildMember member;
                if (cache.TryRemove(guild, userId, out member))
                    return member;
                else
                    throw new DiscordApiCacheHelperException($"Received guild member remove for unknown user with id {userId} "
                        + $"in guild '{guild.Name}'");
            }
            else
                throw new DiscordApiCacheHelperException($"Recieved guild member remove for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Handles adding/updating a chunk of <see cref="DiscordGuildMember"/>s.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> containing the <see cref="DiscordGuildMember"/>s.</param>
        /// <returns>Returns the involved <see cref="DiscordGuild"/> and the added/updated <see cref="DiscordGuildMember"/>s.</returns>
        public Tuple<DiscordGuild, DiscordGuildMember[]> GuildMembersChunk(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                IReadOnlyList<DiscordApiData> members = data.GetArray("members");
                DiscordGuildMember[] finalMembers = new DiscordGuildMember[members.Count];
                for (int i = 0; i < members.Count; i++)
                {
                    DiscordGuildMember member = UpdateGuildMember(members[i]);
                    finalMembers[i] = member;
                }

                return new Tuple<DiscordGuild, DiscordGuildMember[]>(guild, finalMembers);
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild members chunk for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Creates a <see cref="DiscordRole"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the create.</param>
        /// <returns>Returns the involved <see cref="DiscordGuild"/> and the created <see cref="DiscordRole"/>.</returns>
        public Tuple<DiscordGuild, DiscordRole> CreateGuildRole(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                DiscordApiData roleData = data.Get("role");
                string roleId = roleData.GetString("id");
                DiscordRole role = cache.AddOrUpdate(guild, roleId, roleData, () => { return new DiscordRole(); });

                return new Tuple<DiscordGuild, DiscordRole>(guild, role);
            }
            else
                throw new DiscordApiCacheHelperException($"Received role create for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Updates a <see cref="DiscordRole"/> specified from the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the involved <see cref="DiscordGuild"/> and the updated <see cref="DiscordRole"/>.</returns>
        public Tuple<DiscordGuild, DiscordRole> UpdateGuildRole(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                DiscordApiData roleData = data.Get("role");
                string roleId = roleData.GetString("id");
                DiscordRole role = cache.AddOrUpdate(guild, roleId, roleData, () => { return new DiscordRole(); });

                return new Tuple<DiscordGuild, DiscordRole>(guild, role);
            }
            else
                throw new DiscordApiCacheHelperException($"Received role update for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Deletes a <see cref="DiscordRole"/> specified in the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the removal.</param>
        /// <returns>Returns the involved <see cref="DiscordGuild"/> and the deleted <see cref="DiscordRole"/>.</returns>
        public Tuple<DiscordGuild, DiscordRole> DeleteGuildRole(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                DiscordRole role;
                string roleId = data.GetString("role_id");
                if (cache.TryRemove(guild, roleId, out role))
                    return new Tuple<DiscordGuild, DiscordRole>(guild, role);
                else
                    throw new DiscordApiCacheHelperException($"Received role delete for unknown role with id {roleId} in"
                        + $" guild '{guild.Name}'");
            }
            else
                throw new DiscordApiCacheHelperException($"Received role delete for unknown guild with id {guildId}");
        }
        #endregion

        #region Message Events
        /// <summary>
        /// Creates a <see cref="DiscordMessage"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the creation.</param>
        /// <returns>Returns the created <see cref="DiscordMessage"/>.</returns>
        public DiscordMessage CreateMessage(DiscordApiData data)
        {
            DiscordMessage message = new DiscordMessage(client);
            message.Update(data);

            message.Channel?.CacheMessage(message);

            return message;
        }

        /// <summary>
        /// Updates a <see cref="DiscordMessage"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the updated <see cref="DiscordMessage"/>.</returns>
        public DiscordMessage UpdateMessage(DiscordApiData data)
        {
            string channelId = data.GetString("channel_id");
            DiscordChannel channel;
            if (cache.TryGet(channelId, out channel))
            {
                string messageId = data.GetString("id");
                DiscordMessage message;
                if (!channel.TryGetMessage(messageId, out message))
                {
                    message = client.Rest.Messages.Get(channel, messageId).Result;
                }

                message.Update(data);
                return message;
            }

            throw new DiscordApiCacheHelperException(
                $"Attempted to update a message in an unknown channel with id {channelId}");
        }

        /// <summary>
        /// Deletes the <see cref="DiscordMessage"/> specified in the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the deletion.</param>
        /// <returns>Returns the deleted <see cref="DiscordMessage"/>.</returns>
        public DiscordMessage DeleteMessage(DiscordApiData data)
        {
            string channelId = data.GetString("channel_id");
            DiscordChannel channel;
            if (cache.TryGet(channelId, out channel))
            {
                string messageId = data.GetString("id");
                DiscordMessage message;
                if (channel.TryRemoveMessage(messageId, out message))
                    return message;
                else
                    // Null is valid here because once a message is deleted,
                    // the only way to get it is from a local cache.
                    return null;
            }

            throw new DiscordApiCacheHelperException(
                $"Attempted to delete a message in an unknown channel with id {channelId}");
        }

        /// <summary>
        /// Deletes multiple <see cref="DiscordMessage"/>s specified in the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the deletion.</param>
        /// <returns>Returns the deleted <see cref="DiscordMessage"/>s.</returns>
        public DiscordMessage[] DeleteMessageBulk(DiscordApiData data)
        {
            string channelId = data.GetString("channel_id");
            IReadOnlyList<DiscordApiData> ids = data.GetArray("ids");
            DiscordMessage[] messages = new DiscordMessage[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                DiscordApiData id = ids[i];

                DiscordApiData deleteData = new DiscordApiData();
                deleteData.Set("id", id.Value);
                deleteData.Set("channel_id", channelId);

                DiscordMessage msg = DeleteMessage(deleteData);
                messages[i] = msg;
            }

            return messages;
        }
        #endregion

        #region User Events
        /// <summary>
        /// Updates a <see cref="DiscordUser"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the updated <see cref="DiscordUser"/>.</returns>
        public DiscordUser UpdateUser(DiscordApiData data)
        {
            string userId = data.GetString("id");
            DiscordUser user = cache.AddOrUpdate(userId, data, () => { return new DiscordUser(); });

            return user;
        }
        #endregion

        #region Misc Events
        /// <summary>
        /// Updates the presence of a <see cref="DiscordGuildMember"/> based on the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the updated <see cref="DiscordGuildMember"/>.</returns>
        public DiscordGuildMember UpdatePresence(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.TryGet(guildId, out guild))
            {
                string userId = data.LocateString("user.id");
                DiscordGuildMember member;
                if (cache.GetAndTryUpdate(guild, userId, data, out member))
                    return member;
                else
                    throw new DiscordApiCacheHelperException($"Received presence update for unknown user with "
                        + $"id {userId} in guild '{guild.Name}'");
            }
            else
                throw new DiscordApiCacheHelperException($"Received presence update for unknown guild with id {guildId}");
        }

        /// <summary>
        /// Updates a <see cref="DiscordGuildMember"/>s <see cref="DiscordVoiceState"/> based on the given <see cref="DiscordApiData"/>. 
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> specifying the update.</param>
        /// <returns>Returns the <see cref="DiscordGuildMember"/> whose <see cref="DiscordVoiceState"/> was updated.</returns>
        public DiscordGuildMember UpdateVoiceState(DiscordApiData data)
        {
            string userId = data.GetString("user_id");
            string guildId = data.GetString("guild_id");

            if (guildId != null)
            {
                DiscordGuild guild;
                if (cache.TryGet(guildId, out guild))
                {
                    DiscordGuildMember member;
                    if (cache.TryGet(guild, userId, out member))
                    {
                        member.VoiceState.Update(data);
                        return member;
                    }
                    else
                        throw new DiscordApiCacheHelperException($"Recieved voice state update for unknown user with id {userId} in "
                            + $"guild '{guild.Name}'");
                }
                else
                    throw new DiscordApiCacheHelperException($"Received voice state update for unknown guild with id {guildId}");
            }
            else
            {
                // ¯\_(ツ)_/¯
                throw new DiscordApiCacheHelperException("¯\\_(ツ)_/¯");
            }
        }
        #endregion
    }
}
