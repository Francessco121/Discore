using ConcurrentCollections;
using Discore.Voice;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

#pragma warning disable IDE0051 // Remove unused private members

namespace Discore.WebSocket.Internal
{
    partial class Gateway
    {
        #region Public Events       
        public event EventHandler<DMChannelEventArgs>? OnDMChannelCreated;
        public event EventHandler<GuildChannelEventArgs>? OnGuildChannelCreated;
        public event EventHandler<GuildChannelEventArgs>? OnGuildChannelUpdated;
        public event EventHandler<DMChannelEventArgs>? OnDMChannelRemoved;
        public event EventHandler<GuildChannelEventArgs>? OnGuildChannelRemoved;


        public event EventHandler<GuildEventArgs>? OnGuildCreated;
        public event EventHandler<GuildEventArgs>? OnGuildUpdated;
        public event EventHandler<GuildEventArgs>? OnGuildRemoved;

        public event EventHandler<GuildEventArgs>? OnGuildAvailable;
        public event EventHandler<GuildEventArgs>? OnGuildUnavailable;

        public event EventHandler<GuildUserEventArgs>? OnGuildBanAdded;
        public event EventHandler<GuildUserEventArgs>? OnGuildBanRemoved;

        public event EventHandler<GuildEventArgs>? OnGuildEmojisUpdated;

        public event EventHandler<GuildIntegrationsEventArgs>? OnGuildIntegrationsUpdated;

        public event EventHandler<GuildMemberEventArgs>? OnGuildMemberAdded;
        public event EventHandler<GuildMemberEventArgs>? OnGuildMemberRemoved;
        public event EventHandler<GuildMemberEventArgs>? OnGuildMemberUpdated;
        public event EventHandler<GuildMemberChunkEventArgs>? OnGuildMembersChunk;

        public event EventHandler<GuildRoleEventArgs>? OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs>? OnGuildRoleUpdated;
        public event EventHandler<GuildRoleEventArgs>? OnGuildRoleDeleted;

        public event EventHandler<ChannelPinsUpdateEventArgs>? OnChannelPinsUpdated;

        public event EventHandler<MessageEventArgs>? OnMessageCreated;
        public event EventHandler<MessageUpdateEventArgs>? OnMessageUpdated;
        public event EventHandler<MessageDeleteEventArgs>? OnMessageDeleted;
        public event EventHandler<MessageReactionEventArgs>? OnMessageReactionAdded;
        public event EventHandler<MessageReactionEventArgs>? OnMessageReactionRemoved;
        public event EventHandler<MessageReactionRemoveAllEventArgs>? OnMessageAllReactionsRemoved;

        public event EventHandler<WebhooksUpdateEventArgs>? OnWebhookUpdated;

        public event EventHandler<PresenceEventArgs>? OnPresenceUpdated;

        public event EventHandler<TypingStartEventArgs>? OnTypingStarted;

        public event EventHandler<UserEventArgs>? OnUserUpdated;
        public event EventHandler<VoiceStateEventArgs>? OnVoiceStateUpdated;
        #endregion

        void LogServerTrace(string prefix, JsonElement data)
        {
            JsonElement? traceArray = data.GetPropertyOrNull("_trace");
            if (traceArray != null && traceArray.Value.ValueKind == JsonValueKind.Array)
            {
                JsonElement _traceArray = traceArray.Value;
                int numTraces = _traceArray.GetArrayLength();

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < numTraces; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(_traceArray[i].ToString());
                }

                log.LogVerbose($"[{prefix}] trace = {sb}");
            }
        }

        MutableUser CacheUser(JsonElement userData)
        {
            Snowflake userId = userData.GetProperty("id").GetSnowflake();

            // Get user
            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                // Create user
                mutableUser = new MutableUser(userId, false);
                cache.Users[userId] = mutableUser;
            }

            // Update user
            mutableUser.Update(userData);

            return mutableUser;
        }

        MutableGuildMember CacheGuildMember(JsonElement memberData, Snowflake guildId)
        {
            // Cache user first
            MutableUser user = CacheUser(memberData.GetProperty("user"));

            // Get member
            MutableGuildMember? member;
            if (!cache.GuildMembers.TryGetValue(guildId, user.Id, out member))
            {
                // Create member
                member = new MutableGuildMember(user, guildId);
                cache.GuildMembers[guildId, user.Id] = member;
            }

            // Update
            member.Update(memberData);

            return member;
        }

        [DispatchEvent("READY")]
        void HandleReadyEvent(JsonElement data)
        {
            // Check gateway protocol
            int protocolVersion = data.GetProperty("v").GetInt32();
            if (protocolVersion != GATEWAY_VERSION)
                log.LogError($"[Ready] Gateway protocol mismatch! Expected v{GATEWAY_VERSION}, got {protocolVersion}.");

            // Check shard
            if (shard.Id != 0 || totalShards > 1)
            {
                JsonElement? shardData = data.GetPropertyOrNull("shard");
                if (shardData != null)
                {
                    JsonElement _shardData = shardData.Value;
                    int shardDataCount = _shardData.GetArrayLength();

                    if (shardDataCount > 0 && _shardData[0].GetInt32() != shard.Id)
                        log.LogError($"[Ready] Shard ID mismatch! Expected {shard.Id}, got {_shardData[0].GetInt32()}");
                    if (shardDataCount > 1 && _shardData[1].GetInt32() != totalShards)
                        log.LogError($"[Ready] Total shards mismatch! Expected {totalShards}, got {_shardData[1].GetInt32()}");
                }
            }

            // Clear the cache
            cache.Clear();

            // Get the current bot's user object
            MutableUser user = CacheUser(data.GetProperty("user"));

            shard.UserId = user.Id;

            log.LogInfo($"[Ready] user = {user.Username}#{user.Discriminator}");

            // Get session ID
            sessionId = data.GetProperty("session_id").GetString()!;

            // Get unavailable guilds
            foreach (JsonElement unavailableGuildData in data.GetProperty("guilds").EnumerateArray())
            {
                Snowflake guildId = unavailableGuildData.GetProperty("id").GetSnowflake();

                cache.AddGuildId(guildId);
                cache.SetGuildAvailability(guildId, false);
            }

            LogServerTrace("Ready", data);

            // Signal that the connection is ready
            handshakeCompleteEvent.Set();
        }

        [DispatchEvent("RESUMED")]
        void HandleResumedEvent(JsonElement data)
        {
            // Signal that the connection is ready
            handshakeCompleteEvent.Set();

            log.LogInfo("[Resumed] Successfully resumed.");
            LogServerTrace("Resumed", data);
        }

        #region Guild
        [DispatchEvent("GUILD_CREATE")]
        void HandleGuildCreateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("id").GetSnowflake();

            bool wasUnavailable = !cache.IsGuildAvailable(guildId);

            // Update guild
            MutableGuild mutableGuild;
            if (!cache.Guilds.TryGetValue(guildId, out mutableGuild))
            {
                mutableGuild = new MutableGuild(guildId);
                cache.Guilds[guildId] = mutableGuild;
            }

            mutableGuild.Update(data);

            // Ensure the cache guildId list contains this guild (it uses a hashset so don't worry about duplicates).
            cache.AddGuildId(guildId);

            // GUILD_CREATE specifics
            // Update metadata
            cache.GuildMetadata[guildId] = new DiscordGuildMetadata(data);

            // Deserialize members
            cache.GuildMembers.Clear(guildId);
            JsonElement membersArray = data.GetProperty("members");
            int numMembers = membersArray.GetArrayLength();
            for (int i = 0; i < numMembers; i++)
            {
                JsonElement memberData = membersArray[i];
                CacheGuildMember(memberData, guildId);
            }

            // Deserialize channels
            cache.ClearGuildChannels(guildId);
            JsonElement channelsArray = data.GetProperty("channels");
            int numChannels = channelsArray.GetArrayLength();
            for (int i = 0; i < numChannels; i++)
            {
                JsonElement channelData = channelsArray[i];
                DiscordChannelType channelType = (DiscordChannelType)channelData.GetProperty("type").GetInt32();

                DiscordGuildChannel? channel = null;
                if (channelType == DiscordChannelType.GuildText)
                    channel = new DiscordGuildTextChannel(channelData, guildId);
                else if (channelType == DiscordChannelType.GuildVoice)
                    channel = new DiscordGuildVoiceChannel(channelData, guildId);
                else if (channelType == DiscordChannelType.GuildCategory)
                    channel = new DiscordGuildCategoryChannel(channelData, guildId);
                else if (channelType == DiscordChannelType.GuildNews)
                    channel = new DiscordGuildNewsChannel(channelData, guildId);
                else if (channelType == DiscordChannelType.GuildStore)
                    channel = new DiscordGuildStoreChannel(channelData, guildId);

                if (channel != null)
                    cache.AddGuildChannel(channel);
            }

            // Deserialize voice states
            cache.GuildVoiceStates.Clear(guildId);
            JsonElement voiceStatesArray = data.GetProperty("voice_states");
            int numVoiceStates = voiceStatesArray.GetArrayLength();
            for (int i = 0; i < numVoiceStates; i++)
            {
                DiscordVoiceState state = new DiscordVoiceState(voiceStatesArray[i], guildId: guildId);
                UpdateMemberVoiceState(state);
            }

            // Deserialize presences
            cache.GuildPresences.Clear(guildId);
            JsonElement presencesArray = data.GetProperty("presences");
            int numPresences = presencesArray.GetArrayLength();
            for (int i = 0; i < numPresences; i++)
            {
                // Presence's in GUILD_CREATE do not contain full user objects,
                // so don't attempt to update them here.
                var presence = new DiscordUserPresence(presencesArray[i]);
                cache.GuildPresences[guildId, presence.UserId] = presence;
            }

            // Mark the guild as available
            cache.SetGuildAvailability(guildId, true);

            // Fire event
            if (wasUnavailable)
                OnGuildAvailable?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            else
                OnGuildCreated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
        }

        [DispatchEvent("GUILD_UPDATE")]
        void HandleGuildUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("id").GetSnowflake();

            MutableGuild mutableGuild;
            if (cache.Guilds.TryGetValue(guildId, out mutableGuild))
            {
                // Update guild
                mutableGuild.Update(data);

                // Fire event
                OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_DELETE")]
        async Task HandleGuildDeleteEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("id").GetSnowflake();
            bool unavailable = data.GetPropertyOrNull("unavailable")?.GetBooleanOrNull() ?? false;

            if (unavailable)
            {
                // Tell the cache this guild is no longer available
                cache.SetGuildAvailability(guildId, false);

                if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
                {
                    // Fire event
                    OnGuildUnavailable?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
                }
                else
                    throw new ShardCacheException($"Guild {guildId} was not in the cache! unavailable = true");
            }
            else
            {
                // Disconnect the voice connection for this guild if connected.
                if (shard.Voice.TryGetVoiceConnection(guildId, out DiscordVoiceConnection voiceConnection)
                    && voiceConnection.IsConnected)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(5000);

                    await voiceConnection.DisconnectWithReasonAsync(VoiceConnectionInvalidationReason.BotRemovedFromGuild, 
                        cts.Token).ConfigureAwait(false);
                }

                // Clear all cache data related to the guild
                cache.GuildMetadata.TryRemove(guildId, out _);
                cache.GuildMembers.RemoveParent(guildId);
                cache.GuildPresences.RemoveParent(guildId);
                cache.GuildVoiceStates.RemoveParent(guildId);

                if (cache.GuildChannelIds.TryRemove(guildId, out  ConcurrentHashSet<Snowflake> channelIds))
                {
                    foreach (Snowflake channelId in channelIds)
                        cache.GuildChannels.TryRemove(channelId, out _);
                }

                // Remove the guild from cache
                cache.RemoveGuildId(guildId);
                if (cache.Guilds.TryRemove(guildId, out MutableGuild mutableGuild))
                {
                    // Ensure all references are cleared
                    mutableGuild.ClearReferences();

                    // Fire event
                    OnGuildRemoved?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
                }
                else
                    throw new ShardCacheException($"Guild {guildId} was not in the cache! unavailable = false");
            }
        }

        [DispatchEvent("GUILD_BAN_ADD")]
        void HandleGuildBanAddEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            MutableUser mutableUser = CacheUser(data.GetProperty("user"));

            OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guildId, mutableUser.ImmutableEntity));
        }

        [DispatchEvent("GUILD_BAN_REMOVE")]
        void HandleGuildBanRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            MutableUser mutableUser = CacheUser(data.GetProperty("user"));

            OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildId, mutableUser.ImmutableEntity));
        }

        [DispatchEvent("GUILD_EMOJIS_UPDATE")]
        void HandleGuildEmojisUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            { 
                // Clear existing emojis
                mutableGuild.Emojis.Clear();

                // Deseralize new emojis
                JsonElement emojisArray = data.GetProperty("emojis");
                int numEmojis = emojisArray.GetArrayLength();
                for (int i = 0; i < numEmojis; i++)
                {
                    DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                    mutableGuild.Emojis[emoji.Id] = emoji;
                }

                // Dirty the guild
                mutableGuild.Dirty();

                // Invoke the event
                OnGuildEmojisUpdated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_INTEGRATIONS_UPDATE")]
        void HandleGuildIntegrationsUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            OnGuildIntegrationsUpdated?.Invoke(this, new GuildIntegrationsEventArgs(shard, guildId));
        }

        [DispatchEvent("GUILD_MEMBER_ADD")]
        void HandleGuildMemberAddEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            MutableGuildMember mutableMember = CacheGuildMember(data, guildId);

            // Fire event
            OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
        }

        [DispatchEvent("GUILD_MEMBER_REMOVE")]
        void HandleGuildMemberRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            Snowflake userId = CacheUser(data.GetProperty("user")).Id;

            // Get and remove member
            if (cache.GuildMembers.TryRemove(guildId, userId, out MutableGuildMember? mutableMember))
            {
                // Ensure all references are removed
                mutableMember.ClearReferences();

                // Fire event
                OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
            }
        }

        [DispatchEvent("GUILD_MEMBER_UPDATE")]
        void HandleGuildMemberUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            Snowflake userId = CacheUser(data.GetProperty("user")).Id;

            // Get member
            if (cache.GuildMembers.TryGetValue(guildId, userId, out MutableGuildMember? mutableMember))
            {
                // Update member
                mutableMember.PartialUpdate(data);

                // Fire event
                OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
            }

            // It is technically valid for the member to not exist here, especially if the guild is considered large.
        }

        [DispatchEvent("GUILD_MEMBERS_CHUNK")]
        void HandleGuildMembersChunkEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            // Get every member and ensure they are cached
            JsonElement membersData = data.GetProperty("members");
            DiscordGuildMember[] members = new DiscordGuildMember[membersData.GetArrayLength()];
            for (int i = 0; i < members.Length; i++)
            {
                JsonElement memberData = membersData[i];
                MutableGuildMember mutableMember = CacheGuildMember(memberData, guildId);

                members[i] = mutableMember.ImmutableEntity;
            }

            // Fire event
            OnGuildMembersChunk?.Invoke(this, new GuildMemberChunkEventArgs(shard, guildId, members));
        }

        [DispatchEvent("GUILD_ROLE_CREATE")]
        void HandleGuildRoleCreateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                JsonElement roleData = data.GetProperty("role");
                DiscordRole role = new DiscordRole(roleData, guildId: guildId);

                mutableGuild.Roles[role.Id] = role;
                mutableGuild.Dirty();

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_UPDATE")]
        void HandleGuildRoleUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                JsonElement roleData = data.GetProperty("role");
                DiscordRole role = new DiscordRole(roleData, guildId: guildId);

                mutableGuild.Roles[role.Id] = role;
                mutableGuild.Dirty();

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_DELETE")]
        void HandleGuildRoleDeleteEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                Snowflake roleId = data.GetProperty("role_id").GetSnowflake();

                if (mutableGuild.Roles.TryRemove(roleId, out DiscordRole role))
                    OnGuildRoleDeleted?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
                else
                    throw new ShardCacheException($"Role {roleId} was not in the guild {guildId} cache!");
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }
        #endregion

        #region Channel
        [DispatchEvent("CHANNEL_CREATE")]
        void HandleChannelCreateEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            if (type == DiscordChannelType.DirectMessage)
            {
                // DM channel
                MutableUser recipient = CacheUser(data.GetProperty("recipients")[0]);

                MutableDMChannel mutableDMChannel;
                if (!cache.DMChannels.TryGetValue(id, out mutableDMChannel))
                {
                    mutableDMChannel = new MutableDMChannel(id, recipient);
                    cache.DMChannels[id] = mutableDMChannel;
                }

                OnDMChannelCreated?.Invoke(this, new DMChannelEventArgs(shard, mutableDMChannel.ImmutableEntity));
            }
            else if (type == DiscordChannelType.GuildText 
                || type == DiscordChannelType.GuildVoice
                || type == DiscordChannelType.GuildCategory
                || type == DiscordChannelType.GuildNews
                || type == DiscordChannelType.GuildStore)
            {
                // Guild channel
                Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

                DiscordGuildChannel channel;

                if (type == DiscordChannelType.GuildText)
                    channel = new DiscordGuildTextChannel(data, guildId);
                else if (type == DiscordChannelType.GuildVoice)
                    channel = new DiscordGuildVoiceChannel(data, guildId);
                else if (type == DiscordChannelType.GuildCategory)
                    channel = new DiscordGuildCategoryChannel(data, guildId);
                else if (type == DiscordChannelType.GuildNews)
                    channel = new DiscordGuildNewsChannel(data, guildId);
                else if (type == DiscordChannelType.GuildStore)
                    channel = new DiscordGuildStoreChannel(data, guildId);
                else
                    throw new NotImplementedException($"Guild channel type \"{type}\" has no implementation!");

                cache.GuildChannels[id] = channel;

                OnGuildChannelCreated?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
            }
        }

        [DispatchEvent("CHANNEL_UPDATE")]
        void HandleChannelUpdateEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            DiscordGuildChannel? channel = null;

            if (type == DiscordChannelType.GuildText)
                channel = new DiscordGuildTextChannel(data, guildId);
            else if (type == DiscordChannelType.GuildVoice)
                channel = new DiscordGuildVoiceChannel(data, guildId);
            else if (type == DiscordChannelType.GuildCategory)
                channel = new DiscordGuildCategoryChannel(data, guildId);
            else if (type == DiscordChannelType.GuildNews)
                channel = new DiscordGuildNewsChannel(data, guildId);
            else if (type == DiscordChannelType.GuildStore)
                channel = new DiscordGuildStoreChannel(data, guildId);

            if (channel != null)
            {
                cache.GuildChannels[id] = channel;

                OnGuildChannelUpdated?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
            }
            else
                log.LogWarning($"Failed to update channel {id} because the type ({type}) doesn't have an implementation!");
        }

        [DispatchEvent("CHANNEL_DELETE")]
        void HandleChannelDeleteEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            if (type == DiscordChannelType.DirectMessage)
            {
                // DM channel
                DiscordDMChannel dm;
                if (cache.DMChannels.TryRemove(id, out MutableDMChannel mutableDM))
                {
                    mutableDM.ClearReferences();

                    dm = mutableDM.ImmutableEntity;
                }
                else
                    dm = new DiscordDMChannel(data);

                OnDMChannelRemoved?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else if (type == DiscordChannelType.GuildText 
                || type == DiscordChannelType.GuildVoice
                || type == DiscordChannelType.GuildCategory
                || type == DiscordChannelType.GuildNews
                || type == DiscordChannelType.GuildStore)
            {
                // Guild channel
                Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

                DiscordGuildChannel channel;

                if (type == DiscordChannelType.GuildText)
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildTextChannel(data, guildId);
                }
                else if (type == DiscordChannelType.GuildVoice)
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildVoiceChannel(data, guildId);
                }
                else if (type == DiscordChannelType.GuildCategory)
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildCategoryChannel(data, guildId);
                }
                else if (type == DiscordChannelType.GuildNews)
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildNewsChannel(data, guildId);
                }
                else if (type == DiscordChannelType.GuildStore)
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildStoreChannel(data, guildId);
                }
                else
                    throw new NotImplementedException($"Guild channel type \"{type}\" has no implementation!");

                OnGuildChannelRemoved?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
            }
        }

        [DispatchEvent("CHANNEL_PINS_UPDATE")]
        void HandleChannelPinsUpdateEvent(JsonElement data)
        {
            DateTime? lastPinTimestamp = data.GetPropertyOrNull("last_pin_timestamp")?.GetDateTimeOrNull();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnChannelPinsUpdated?.Invoke(this, new ChannelPinsUpdateEventArgs(shard, channelId, lastPinTimestamp));
        }
        #endregion

        #region Message
        [DispatchEvent("MESSAGE_CREATE")]
        void HandleMessageCreateEvent(JsonElement data)
        {
            // Get author
            JsonElement authorData = data.GetProperty("author");
            Snowflake authorId = authorData.GetProperty("id").GetSnowflake();
            bool isWebhookUser = !string.IsNullOrWhiteSpace(data.GetPropertyOrNull("webhook_id")?.GetString());

            MutableUser mutableAuthor;
            if (!cache.Users.TryGetValue(authorId, out mutableAuthor))
            {
                mutableAuthor = new MutableUser(authorId, isWebhookUser);
                cache.Users[authorId] = mutableAuthor;
            }

            mutableAuthor.Update(authorData);

            // Get mentioned users
            JsonElement mentionsArray = data.GetProperty("mentions");
            int numMentions = mentionsArray.GetArrayLength();
            for (int i = 0; i < numMentions; i++)
            {
                CacheUser(mentionsArray[i]);
            }

            // Create message
            DiscordMessage message = new DiscordMessage(data);

            OnMessageCreated?.Invoke(this, new MessageEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_UPDATE")]
        void HandleMessageUpdateEvent(JsonElement data)
        {
            // Get author
            JsonElement? authorData = data.GetPropertyOrNull("author");
            if (authorData != null)
            {
                JsonElement _authorData = authorData.Value;

                Snowflake authorId = _authorData.GetProperty("id").GetSnowflake();
                bool isWebhookUser = !string.IsNullOrWhiteSpace(data.GetPropertyOrNull("webhook_id")?.GetString());

                MutableUser mutableAuthor;
                if (!cache.Users.TryGetValue(authorId, out mutableAuthor))
                {
                    mutableAuthor = new MutableUser(authorId, isWebhookUser);
                    cache.Users[authorId] = mutableAuthor;
                }

                mutableAuthor.Update(_authorData);
            }

            // Get mentioned users
            JsonElement? mentionsArray = data.GetPropertyOrNull("mentions");
            if (mentionsArray != null)
            {
                JsonElement _mentionsArray = mentionsArray.Value;
                int numMentions = _mentionsArray.GetArrayLength();

                for (int i = 0; i < numMentions; i++)
                {
                    CacheUser(_mentionsArray[i]);
                }
            }

            // Create message
            DiscordPartialMessage message = new DiscordPartialMessage(data);

            OnMessageUpdated?.Invoke(this, new MessageUpdateEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_DELETE")]
        void HandleMessageDeleteEvent(JsonElement data)
        {
            Snowflake messageId = data.GetProperty("id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
        }

        [DispatchEvent("MESSAGE_DELETE_BULK")]
        void HandleMessageDeleteBulkEvent(JsonElement data)
        {
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            JsonElement idArray = data.GetProperty("ids");
            int numIds = idArray.GetArrayLength();
            for (int i = 0; i < numIds; i++)
            {
                Snowflake messageId = idArray[i].GetSnowflake();
                OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
            }
        }

        [DispatchEvent("MESSAGE_REACTION_ADD")]
        void HandleMessageReactionAddEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();
            JsonElement emojiData = data.GetProperty("emoji");

            DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionAdded?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE")]
        void HandleMessageReactionRemoveEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();
            JsonElement emojiData = data.GetProperty("emoji");

            DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionRemoved?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE_ALL")]
        void HandleMessageReactionRemoveAllEvent(JsonElement data)
        {
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();

            OnMessageAllReactionsRemoved?.Invoke(this, new MessageReactionRemoveAllEventArgs(shard, messageId, channelId));
        }
        #endregion

        [DispatchEvent("WEBHOOKS_UPDATE")]
        void HandleWebhooksUpdate(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnWebhookUpdated?.Invoke(this, new WebhooksUpdateEventArgs(shard, guildId, channelId));
        }

        [DispatchEvent("PRESENCE_UPDATE")]
        void HandlePresenceUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            // Update user
            JsonElement userData = data.GetProperty("user");
            Snowflake userId = userData.GetProperty("id").GetSnowflake();

            if (cache.Users.TryGetValue(userId, out MutableUser mutableUser))
            {
                mutableUser.PartialUpdate(userData);
            }
            else
                // Don't throw exception since we can still update everything else...
                log.LogError($"[PRESENCE_UPDATE] Failed to update user {userId}, they were not in the cache!");

            // Update presence
            DiscordUserPresence presence = new DiscordUserPresence(data);
            cache.GuildPresences[guildId, userId] = presence;

            // Update member
            if (cache.GuildMembers.TryGetValue(guildId, userId, out MutableGuildMember? mutableMember))
            {
                mutableMember.PartialUpdate(data);

                // Fire event
                OnPresenceUpdated?.Invoke(this, new PresenceEventArgs(shard, guildId, mutableMember.ImmutableEntity, presence));
            }

            // It is technically valid for the member to not exist here, especially if the guild is considered large.
        }

        [DispatchEvent("TYPING_START")]
        void HandleTypingStartEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            int timestamp = data.GetProperty("timestamp").GetInt32();

            OnTypingStarted?.Invoke(this, new TypingStartEventArgs(shard, userId, channelId, timestamp));
        }

        [DispatchEvent("USER_UPDATE")]
        void HandleUserUpdateEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("id").GetSnowflake();

            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(data);

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, mutableUser.ImmutableEntity));
        }

        #region Voice
        /// <summary>
        /// Handles updating the cache list of members connected to voice channels, as well as updating the voice state.
        /// </summary>
        void UpdateMemberVoiceState(DiscordVoiceState newState)
        {
            // Save previous state
            DiscordVoiceState? previousState = cache.GuildVoiceStates[newState.GuildId!.Value, newState.UserId];

            // Update cache with new state
            cache.GuildVoiceStates[newState.GuildId.Value, newState.UserId] = newState;

            // If previously in a voice channel that differs from the new channel (or no longer in a channel),
            // then remove this user from the voice channel user list.
            if (previousState != null && previousState.ChannelId.HasValue && previousState.ChannelId != newState.ChannelId)
            {
                shard.Voice.RemoveUserFromVoiceChannel(previousState.ChannelId.Value, newState.UserId);
            }

            // If user is now in a voice channel, add them to the user list.
            if (newState.ChannelId.HasValue)
            {
                shard.Voice.AddUserToVoiceChannel(newState.ChannelId.Value, newState.UserId);
            }
        }

        [DispatchEvent("VOICE_STATE_UPDATE")]
        async Task HandleVoiceStateUpdateEvent(JsonElement data)
        {
            Snowflake? guildId = data.GetPropertyOrNull("guild_id")?.GetSnowflake();
            if (guildId.HasValue) // Only guild voice channels are supported.
            {
                Snowflake userId = data.GetProperty("user_id").GetSnowflake();

                // Update the voice state
                DiscordVoiceState voiceState = new DiscordVoiceState(data, guildId: guildId.Value);
                UpdateMemberVoiceState(voiceState);

                // If this voice state belongs to the current bot,
                // then we need to notify the connection of the session ID.
                if (userId == shard.UserId)
                {
                    DiscordVoiceConnection connection;
                    if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                    {
                        if (voiceState.ChannelId.HasValue)
                        {
                            // Notify the connection of the new state
                            await connection.OnVoiceStateUpdated(voiceState).ConfigureAwait(false);
                        }
                        else if (connection.IsConnected)
                        {
                            // The user has left the channel, so make sure they are disconnected.
                            await connection.DisconnectAsync().ConfigureAwait(false);
                        }
                    }
                }

                // Fire event
                OnVoiceStateUpdated?.Invoke(this, new VoiceStateEventArgs(shard, voiceState));
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported.");
        }

        [DispatchEvent("VOICE_SERVER_UPDATE")]
        async Task HandleVoiceServerUpdateEvent(JsonElement data)
        {
            Snowflake? guildId = data.GetPropertyOrNull("guild_id")?.GetSnowflake();
            if (guildId.HasValue) // Only guild voice channels are supported.
            {
                string token = data.GetProperty("token").GetString()!;
                string endpoint = data.GetProperty("endpoint").GetString()!;

                DiscordVoiceConnection connection;
                if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                {
                    // Notify the connection of the server update
                    await connection.OnVoiceServerUpdated(token, endpoint).ConfigureAwait(false);
                }
                else
                    throw new ShardCacheException($"Voice connection for guild {guildId.Value} was not in the cache!");
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported.");
        }
        #endregion
    }
}

#pragma warning restore IDE0051 // Remove unused private members

#nullable restore
