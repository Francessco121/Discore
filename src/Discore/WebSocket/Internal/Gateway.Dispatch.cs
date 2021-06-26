using ConcurrentCollections;
using Discore.Voice;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members

namespace Discore.WebSocket.Internal
{
    partial class Gateway
    {
        #region Public Events       
        public event EventHandler<ReadyEventArgs>? OnReady;

        public event EventHandler<ChannelEventArgs>? OnChannelCreated;
        public event EventHandler<ChannelEventArgs>? OnChannelUpdated;
        public event EventHandler<ChannelEventArgs>? OnChannelDeleted;


        public event EventHandler<GuildCreateEventArgs>? OnGuildCreated;
        public event EventHandler<GuildUpdateEventArgs>? OnGuildUpdated;
        public event EventHandler<GuildDeleteEventArgs>? OnGuildDeleted;

        public event EventHandler<GuildUserEventArgs>? OnGuildBanAdded;
        public event EventHandler<GuildUserEventArgs>? OnGuildBanRemoved;

        public event EventHandler<GuildEmojisEventArgs>? OnGuildEmojisUpdated;

        public event EventHandler<GuildIntegrationsEventArgs>? OnGuildIntegrationsUpdated;

        public event EventHandler<GuildMemberEventArgs>? OnGuildMemberAdded;
        public event EventHandler<GuildUserEventArgs>? OnGuildMemberRemoved;
        public event EventHandler<GuildMemberUpdateEventArgs>? OnGuildMemberUpdated;
        public event EventHandler<GuildMemberChunkEventArgs>? OnGuildMembersChunk;

        public event EventHandler<GuildRoleEventArgs>? OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs>? OnGuildRoleUpdated;
        public event EventHandler<GuildRoleIdEventArgs>? OnGuildRoleDeleted;

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

        [DispatchEvent("READY")]
        void HandleReadyEvent(JsonElement data)
        {
            // Check gateway protocol
            int protocolVersion = data.GetProperty("v").GetInt32();
            if (protocolVersion != GATEWAY_VERSION)
                log.LogError($"[Ready] Gateway protocol mismatch! Expected v{GATEWAY_VERSION}, got {protocolVersion}.");

            // Check shard
            int? _shardId = null;
            int? _totalShards = null;

            if (shard.Id != 0 || totalShards > 1)
            {
                JsonElement? shardData = data.GetPropertyOrNull("shard");
                if (shardData != null)
                {
                    JsonElement _shardData = shardData.Value;
                    int shardDataCount = _shardData.GetArrayLength();

                    if (shardDataCount > 0)
                    {
                        _shardId = _shardData[0].GetInt32();

                        if (_shardId.Value != shard.Id)
                            log.LogError($"[Ready] Shard ID mismatch! Expected {shard.Id}, got {_shardData[0].GetInt32()}");
                    }

                    if (shardDataCount > 1)
                    {
                        _totalShards = _shardData[1].GetInt32();

                        if (_totalShards.Value != totalShards)
                            log.LogError($"[Ready] Total shards mismatch! Expected {totalShards}, got {_shardData[1].GetInt32()}");
                    }
                }
            }

            // Get the current bot's user object
            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            shard.UserId = user.Id;

            log.LogInfo($"[Ready] user = {user.Username}#{user.Discriminator}");

            // Get session ID
            sessionId = data.GetProperty("session_id").GetString()!;

            // Get unavailable guilds
            unavailableGuildIds.Clear();

            JsonElement guildsData = data.GetProperty("guilds");
            var guildIds = new Snowflake[guildsData.GetArrayLength()];

            for (int i = 0; i < guildIds.Length; i++)
            {
                Snowflake guildId = guildsData[i].GetProperty("id").GetSnowflake();
                guildIds[i] = guildId;

                unavailableGuildIds.Add(guildId);
            }

            LogServerTrace("Ready", data);

            // Signal that the connection is ready
            handshakeCompleteEvent.Set();

            // Fire event
            OnReady?.Invoke(this, new ReadyEventArgs(shard, user, guildIds, _shardId, _totalShards));
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

            bool wasUnavailable = unavailableGuildIds.TryRemove(guildId);

            // Deserialize guild
            var guild = new DiscordGuild(data);

            // GUILD_CREATE specifics

            // Deserialize metadata
            var guildMetadata = new DiscordGuildMetadata(data);

            // Deserialize members
            JsonElement membersArray = data.GetProperty("members");
            var members = new DiscordGuildMember[membersArray.GetArrayLength()];

            for (int i = 0; i < members.Length; i++)
            {
                members[i] = new DiscordGuildMember(membersArray[i], guildId);
            }

            // Deserialize channels
            JsonElement channelsArray = data.GetProperty("channels");
            var channels = new DiscordGuildChannel[channelsArray.GetArrayLength()];

            for (int i = 0; i < channels.Length; i++)
            {
                JsonElement channelData = channelsArray[i];
                DiscordChannelType channelType = (DiscordChannelType)channelData.GetProperty("type").GetInt32();

                DiscordGuildChannel channel;
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
                else
                    channel = new DiscordGuildChannel(channelData, channelType, guildId);

                channels[i] = channel;
            }

            // Deserialize voice states
            this.voiceStates.TryRemove(guildId, out _);

            JsonElement voiceStatesArray = data.GetProperty("voice_states");
            var voiceStates = new DiscordVoiceState[voiceStatesArray.GetArrayLength()];

            for (int i = 0; i < voiceStates.Length; i++)
            {
                var state = new DiscordVoiceState(voiceStatesArray[i], guildId: guildId);
                voiceStates[i] = state;

                UpdateMemberVoiceState(state);
            }

            // Deserialize presences
            JsonElement presencesArray = data.GetProperty("presences");
            var presences = new DiscordUserPresence[presencesArray.GetArrayLength()];

            for (int i = 0; i < presences.Length; i++)
            {
                presences[i] = new DiscordUserPresence(presencesArray[i], guildId);
            }

            // Fire event
            OnGuildCreated?.Invoke(this, new GuildCreateEventArgs(
                shard,
                becameAvailable: wasUnavailable,
                guild,
                guildMetadata,
                members,
                channels,
                voiceStates,
                presences));
        }

        [DispatchEvent("GUILD_UPDATE")]
        void HandleGuildUpdateEvent(JsonElement data)
        {
            // Deserialize guild
            var guild = new DiscordGuild(data);

            // Fire event
            OnGuildUpdated?.Invoke(this, new GuildUpdateEventArgs(shard, guild));
        }

        [DispatchEvent("GUILD_DELETE")]
        async Task HandleGuildDeleteEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("id").GetSnowflake();
            bool unavailable = data.GetPropertyOrNull("unavailable")?.GetBooleanOrNull() ?? false;

            if (unavailable)
            {
                // Mark guild as unavailable
                unavailableGuildIds.Add(guildId);
            }
            else
            {
                // Disconnect the voice connection for this guild if connected.
                if (shard.Voice.TryGetVoiceConnection(guildId, out DiscordVoiceConnection? voiceConnection)
                    && voiceConnection.IsConnected)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(5000);

                    await voiceConnection.DisconnectWithReasonAsync(VoiceConnectionInvalidationReason.BotRemovedFromGuild, 
                        cts.Token).ConfigureAwait(false);
                }
            }

            // Fire event
            OnGuildDeleted?.Invoke(this, new GuildDeleteEventArgs(shard, guildId, unavailable));
        }

        [DispatchEvent("GUILD_BAN_ADD")]
        void HandleGuildBanAddEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guildId, user));
        }

        [DispatchEvent("GUILD_BAN_REMOVE")]
        void HandleGuildBanRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildId, user));
        }

        [DispatchEvent("GUILD_EMOJIS_UPDATE")]
        void HandleGuildEmojisUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            // Deseralize new emojis
            JsonElement emojisArray = data.GetProperty("emojis");
            var emojis = new DiscordEmoji[emojisArray.GetArrayLength()];

            for (int i = 0; i < emojis.Length; i++)
            {
                emojis[i] = new DiscordEmoji(emojisArray[i]);
            }

            // Fire event
            OnGuildEmojisUpdated?.Invoke(this, new GuildEmojisEventArgs(shard, guildId, emojis));
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

            var member = new DiscordGuildMember(data, guildId);

            // Fire event
            OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildId, member));
        }

        [DispatchEvent("GUILD_MEMBER_REMOVE")]
        void HandleGuildMemberRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            // Fire event
            OnGuildMemberRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildId, user));
        }

        [DispatchEvent("GUILD_MEMBER_UPDATE")]
        void HandleGuildMemberUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var partialMember = new DiscordPartialGuildMember(data, guildId);

            // Fire event
            OnGuildMemberUpdated?.Invoke(this, new GuildMemberUpdateEventArgs(shard, guildId, partialMember));
        }

        [DispatchEvent("GUILD_MEMBERS_CHUNK")]
        void HandleGuildMembersChunkEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            // Get every member
            JsonElement membersData = data.GetProperty("members");
            var members = new DiscordGuildMember[membersData.GetArrayLength()];

            for (int i = 0; i < members.Length; i++)
            {
                members[i] = new DiscordGuildMember(membersData[i], guildId);
            }

            // Fire event
            OnGuildMembersChunk?.Invoke(this, new GuildMemberChunkEventArgs(shard, guildId, members));
        }

        [DispatchEvent("GUILD_ROLE_CREATE")]
        void HandleGuildRoleCreateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var role = new DiscordRole(data.GetProperty("role"), guildId: guildId);

            // Fire event
            OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, guildId, role));
        }

        [DispatchEvent("GUILD_ROLE_UPDATE")]
        void HandleGuildRoleUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var role = new DiscordRole(data.GetProperty("role"), guildId: guildId);

            // Fire event
            OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, guildId, role));
        }

        [DispatchEvent("GUILD_ROLE_DELETE")]
        void HandleGuildRoleDeleteEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();
            Snowflake roleId = data.GetProperty("role_id").GetSnowflake();

            // Fire event
            OnGuildRoleDeleted?.Invoke(this, new GuildRoleIdEventArgs(shard, guildId, roleId));
        }
        #endregion

        #region Channel
        [DispatchEvent("CHANNEL_CREATE")]
        void HandleChannelCreateEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            DiscordChannel channel;

            if (type == DiscordChannelType.DirectMessage)
            {
                // DM channel
                channel = new DiscordDMChannel(data);
            }
            else if (data.HasProperty("guild_id"))
            {
                // Guild channel
                Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

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
                    channel = new DiscordGuildChannel(data, type, guildId);
            }
            else
            {
                // Fallback
                channel = new DiscordChannel(data, type);
            }

            // Fire event
            OnChannelCreated?.Invoke(this, new ChannelEventArgs(shard, channel));
        }

        [DispatchEvent("CHANNEL_UPDATE")]
        void HandleChannelUpdateEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();
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
                channel = new DiscordGuildChannel(data, type, guildId);

            // Fire event
            OnChannelUpdated?.Invoke(this, new ChannelEventArgs(shard, channel));
        }

        [DispatchEvent("CHANNEL_DELETE")]
        void HandleChannelDeleteEvent(JsonElement data)
        {
            Snowflake id = data.GetProperty("id").GetSnowflake();
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            DiscordChannel channel;

            if (type == DiscordChannelType.DirectMessage)
            {
                // DM channel
                channel = new DiscordDMChannel(data);
            }
            else if (data.HasProperty("guild_id"))
            {
                // Guild channel
                Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

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
                    channel = new DiscordGuildChannel(data, type, guildId);
            }
            else
            {
                // Fallback
                channel = new DiscordChannel(data, type);
            }

            // Fire event
            OnChannelDeleted?.Invoke(this, new ChannelEventArgs(shard, channel));
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
            // Deserialize message
            var message = new DiscordMessage(data);

            OnMessageCreated?.Invoke(this, new MessageEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_UPDATE")]
        void HandleMessageUpdateEvent(JsonElement data)
        {
            // Deserialize message
            var message = new DiscordPartialMessage(data);

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

                // TODO: Fire OnMessageDeletedBulk event
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

            var presence = new DiscordUserPresence(data, guildId);

            // Fire event
            OnPresenceUpdated?.Invoke(this, new PresenceEventArgs(shard, guildId, presence));
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
            var user = new DiscordUser(data, isWebhookUser: false);

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, user));
        }

        #region Voice
        /// <summary>
        /// Handles updating the cache list of members connected to voice channels, as well as updating the voice state.
        /// </summary>
        void UpdateMemberVoiceState(DiscordVoiceState newState)
        {
            ConcurrentDictionary<Snowflake, DiscordVoiceState> guildVoiceStates;
            if (!voiceStates.TryGetValue(newState.GuildId!.Value, out guildVoiceStates))
            {
                guildVoiceStates = new ConcurrentDictionary<Snowflake, DiscordVoiceState>();
                voiceStates[newState.GuildId.Value] = guildVoiceStates;
            }

            // Save previous state
            DiscordVoiceState? previousState;
            guildVoiceStates.TryGetValue(newState.UserId, out previousState);

            // Update cache with new state
            guildVoiceStates[newState.UserId] = newState;

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
                    DiscordVoiceConnection? connection;
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

                DiscordVoiceConnection? connection;
                if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                {
                    // Notify the connection of the server update
                    await connection.OnVoiceServerUpdated(token, endpoint).ConfigureAwait(false);
                }
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported.");
        }
        #endregion
    }
}

#pragma warning restore IDE0051 // Remove unused private members
