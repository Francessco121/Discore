using System;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordApiCacheHelperException : DiscordioException
    {
        public DiscordApiCacheHelperException(string message) 
            : base(message)
        { }
    }

    public class DiscordApiCacheHelper
    {
        DiscordApiCache cache;
        IDiscordClient client;

        public DiscordApiCacheHelper(IDiscordClient client, DiscordApiCache cache)
        {
            this.client = client;
            this.cache = cache;
        }

        #region Channel Events
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
        public DiscordGuild CreateGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild createdGuild = cache.AddOrUpdate(guildId, data, () => { return new DiscordGuild(client); });

            return createdGuild;
        }

        public DiscordGuild UpdateGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild updatedGuild = cache.AddOrUpdate(guildId, data, () => { return new DiscordGuild(client); });

            return updatedGuild;
        }

        public DiscordGuild DeleteGuild(DiscordApiData data)
        {
            string guildId = data.GetString("id");
            DiscordGuild deletedGuild;
            if (cache.TryRemove(guildId, out deletedGuild))
                return deletedGuild;
            else
                throw new DiscordApiCacheHelperException($"Received delete for non-existant guild id {guildId}");
        }

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

        public DiscordGuild UpdateGuildIntegrations(DiscordApiData data)
        {
            string guildId = data.GetString("guild_id");
            DiscordGuild guild;
            if (cache.GetAndTryUpdate(guildId, data, out guild))
            {
                // TODO: ??

                return guild;
            }
            else
                throw new DiscordApiCacheHelperException($"Received guild integrations update for unknown guild with id {guildId}");
        }

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
        public DiscordMessage CreateMessage(DiscordApiData data)
        {
            DiscordMessage message = new DiscordMessage(client);
            message.Update(data);

            message.Channel?.CacheMessage(message);

            return message;
        }

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
        public DiscordUser UpdateUser(DiscordApiData data)
        {
            string userId = data.GetString("id");
            DiscordUser user = cache.AddOrUpdate(userId, data, () => { return new DiscordUser(); });

            return user;
        }
        #endregion

        #region Misc Events
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
