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

        public event EventHandler<ChannelCreateEventArgs>? OnChannelCreate;
        public event EventHandler<ChannelUpdateEventArgs>? OnChannelUpdate;
        public event EventHandler<ChannelDeleteEventArgs>? OnChannelDelete;

        public event EventHandler<GuildCreateEventArgs>? OnGuildCreate;
        public event EventHandler<GuildUpdateEventArgs>? OnGuildUpdate;
        public event EventHandler<GuildDeleteEventArgs>? OnGuildDelete;

        public event EventHandler<GuildBanAddEventArgs>? OnGuildBanAdd;
        public event EventHandler<GuildBanRemoveEventArgs>? OnGuildBanRemove;

        public event EventHandler<GuildEmojisUpdateEventArgs>? OnGuildEmojisUpdate;

        public event EventHandler<GuildIntegrationsUpdateEventArgs>? OnGuildIntegrationsUpdate;

        public event EventHandler<GuildMemberAddEventArgs>? OnGuildMemberAdd;
        public event EventHandler<GuildMemberRemoveEventArgs>? OnGuildMemberRemove;
        public event EventHandler<GuildMemberUpdateEventArgs>? OnGuildMemberUpdate;
        public event EventHandler<GuildMemberChunkEventArgs>? OnGuildMembersChunk;

        public event EventHandler<GuildRoleCreateEventArgs>? OnGuildRoleCreate;
        public event EventHandler<GuildRoleUpdateEventArgs>? OnGuildRoleUpdate;
        public event EventHandler<GuildRoleDeleteEventArgs>? OnGuildRoleDelete;

        public event EventHandler<ChannelPinsUpdateEventArgs>? OnChannelPinsUpdate;

        public event EventHandler<MessageCreateEventArgs>? OnMessageCreate;
        public event EventHandler<MessageUpdateEventArgs>? OnMessageUpdate;
        public event EventHandler<MessageDeleteEventArgs>? OnMessageDelete;
        public event EventHandler<MessageReactionAddEventArgs>? OnMessageReactionAdd;
        public event EventHandler<MessageReactionRemoveEventArgs>? OnMessageReactionRemove;
        public event EventHandler<MessageReactionRemoveAllEventArgs>? OnMessageReactionRemoveAll;

        public event EventHandler<WebhooksUpdateEventArgs>? OnWebhookUpdate;

        public event EventHandler<PresenceUpdateEventArgs>? OnPresenceUpdate;

        public event EventHandler<TypingStartEventArgs>? OnTypingStart;

        public event EventHandler<UserUpdateEventArgs>? OnUserUpdate;
        public event EventHandler<VoiceStateUpdateEventArgs>? OnVoiceStateUpdate;
        #endregion

        void LogServerTrace(string prefix, JsonElement data)
        {
            JsonElement? traceArray = data.GetPropertyOrNull("_trace");
            if (traceArray != null && traceArray.Value.ValueKind == JsonValueKind.Array)
            {
                JsonElement _traceArray = traceArray.Value;
                int numTraces = _traceArray.GetArrayLength();

                var sb = new StringBuilder();

                for (int i = 0; i < numTraces; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(_traceArray[i].ToString());
                }

                log.LogVerbose($"[{prefix}] trace = {sb}");
            }
        }

        DiscordChannel DeserializeChannel(JsonElement data)
        {
            if (data.HasProperty("guild_id"))
            {
                // Guild channel
                return DeserializeGuildChannel(data, data.GetProperty("guild_id").GetSnowflake());
            }
            else
            {
                var type = (DiscordChannelType)data.GetProperty("type").GetInt32();

                if (type == DiscordChannelType.DirectMessage)
                {
                    // DM channel
                    return new DiscordDMChannel(data);
                }
                else
                {
                    // Fallback
                    return new DiscordChannel(data, type);
                }
            }
        }

        DiscordGuildChannel DeserializeGuildChannel(JsonElement data, Snowflake guildId)
        {
            var type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            return type switch
            {
                DiscordChannelType.GuildText => new DiscordGuildTextChannel(data, guildId),
                DiscordChannelType.GuildVoice => new DiscordGuildVoiceChannel(data, guildId),
                DiscordChannelType.GuildCategory => new DiscordGuildCategoryChannel(data, guildId),
                DiscordChannelType.GuildNews => new DiscordGuildNewsChannel(data, guildId),
                DiscordChannelType.GuildStore => new DiscordGuildStoreChannel(data, guildId),
                _ => new DiscordGuildChannel(data, type, guildId)
            };
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

            // Get resume URL
            resumeGatewayUrl = data.GetProperty("resume_gateway_url").GetString()!;

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

            // If this is from an automatic reconnection, fire OnReconnected
            if (state == GatewayState.Connected)
            {
                log.LogInfo("[Ready] Successfully reconnected with a new session.");
                OnReconnected?.Invoke(this, new GatewayReconnectedEventArgs(isNewSession: true));
            }

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

            // Fire OnReconnected
            OnReconnected?.Invoke(this, new GatewayReconnectedEventArgs(isNewSession: false));
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
                channels[i] = DeserializeGuildChannel(channelsArray[i], guildId);
            }

            // Deserialize voice states
            this.voiceStates.TryRemove(guildId, out _);

            JsonElement voiceStatesArray = data.GetProperty("voice_states");
            var voiceStates = new DiscordVoiceState[voiceStatesArray.GetArrayLength()];

            for (int i = 0; i < voiceStates.Length; i++)
            {
                var state = new DiscordVoiceState(voiceStatesArray[i], guildId: guildId);
                voiceStates[i] = state;
            }

            // Deserialize presences
            JsonElement presencesArray = data.GetProperty("presences");
            var presences = new DiscordUserPresence[presencesArray.GetArrayLength()];

            for (int i = 0; i < presences.Length; i++)
            {
                presences[i] = new DiscordUserPresence(presencesArray[i], guildId);
            }

            // Fire event
            OnGuildCreate?.Invoke(this, new GuildCreateEventArgs(
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
            OnGuildUpdate?.Invoke(this, new GuildUpdateEventArgs(shard, guild));
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
            OnGuildDelete?.Invoke(this, new GuildDeleteEventArgs(shard, guildId, unavailable));
        }

        [DispatchEvent("GUILD_BAN_ADD")]
        void HandleGuildBanAddEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            OnGuildBanAdd?.Invoke(this, new GuildBanAddEventArgs(shard, guildId, user));
        }

        [DispatchEvent("GUILD_BAN_REMOVE")]
        void HandleGuildBanRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            OnGuildBanRemove?.Invoke(this, new GuildBanRemoveEventArgs(shard, guildId, user));
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
            OnGuildEmojisUpdate?.Invoke(this, new GuildEmojisUpdateEventArgs(shard, guildId, emojis));
        }

        [DispatchEvent("GUILD_INTEGRATIONS_UPDATE")]
        void HandleGuildIntegrationsUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            OnGuildIntegrationsUpdate?.Invoke(this, new GuildIntegrationsUpdateEventArgs(shard, guildId));
        }

        [DispatchEvent("GUILD_MEMBER_ADD")]
        void HandleGuildMemberAddEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var member = new DiscordGuildMember(data, guildId);

            // Fire event
            OnGuildMemberAdd?.Invoke(this, new GuildMemberAddEventArgs(shard, guildId, member));
        }

        [DispatchEvent("GUILD_MEMBER_REMOVE")]
        void HandleGuildMemberRemoveEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var user = new DiscordUser(data.GetProperty("user"), isWebhookUser: false);

            // Fire event
            OnGuildMemberRemove?.Invoke(this, new GuildMemberRemoveEventArgs(shard, guildId, user));
        }

        [DispatchEvent("GUILD_MEMBER_UPDATE")]
        void HandleGuildMemberUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var partialMember = new DiscordPartialGuildMember(data, guildId);

            // Fire event
            OnGuildMemberUpdate?.Invoke(this, new GuildMemberUpdateEventArgs(shard, guildId, partialMember));
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
            OnGuildRoleCreate?.Invoke(this, new GuildRoleCreateEventArgs(shard, guildId, role));
        }

        [DispatchEvent("GUILD_ROLE_UPDATE")]
        void HandleGuildRoleUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var role = new DiscordRole(data.GetProperty("role"), guildId: guildId);

            // Fire event
            OnGuildRoleUpdate?.Invoke(this, new GuildRoleUpdateEventArgs(shard, guildId, role));
        }

        [DispatchEvent("GUILD_ROLE_DELETE")]
        void HandleGuildRoleDeleteEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();
            Snowflake roleId = data.GetProperty("role_id").GetSnowflake();

            // Fire event
            OnGuildRoleDelete?.Invoke(this, new GuildRoleDeleteEventArgs(shard, guildId, roleId));
        }
        #endregion

        #region Channel
        [DispatchEvent("CHANNEL_CREATE")]
        void HandleChannelCreateEvent(JsonElement data)
        {
            // Deserialize channel
            DiscordChannel channel = DeserializeChannel(data);

            // Fire event
            OnChannelCreate?.Invoke(this, new ChannelCreateEventArgs(shard, channel));
        }

        [DispatchEvent("CHANNEL_UPDATE")]
        void HandleChannelUpdateEvent(JsonElement data)
        {
            // Deserialize channel
            DiscordChannel channel = DeserializeChannel(data);

            // Fire event
            OnChannelUpdate?.Invoke(this, new ChannelUpdateEventArgs(shard, channel));
        }

        [DispatchEvent("CHANNEL_DELETE")]
        void HandleChannelDeleteEvent(JsonElement data)
        {
            // Deserialize channel
            DiscordChannel channel = DeserializeChannel(data);

            // Fire event
            OnChannelDelete?.Invoke(this, new ChannelDeleteEventArgs(shard, channel));
        }

        [DispatchEvent("CHANNEL_PINS_UPDATE")]
        void HandleChannelPinsUpdateEvent(JsonElement data)
        {
            DateTime? lastPinTimestamp = data.GetPropertyOrNull("last_pin_timestamp")?.GetDateTimeOrNull();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnChannelPinsUpdate?.Invoke(this, new ChannelPinsUpdateEventArgs(shard, channelId, lastPinTimestamp));
        }
        #endregion

        #region Message
        [DispatchEvent("MESSAGE_CREATE")]
        void HandleMessageCreateEvent(JsonElement data)
        {
            // Deserialize message
            var message = new DiscordMessage(data);

            OnMessageCreate?.Invoke(this, new MessageCreateEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_UPDATE")]
        void HandleMessageUpdateEvent(JsonElement data)
        {
            // Deserialize message
            var message = new DiscordPartialMessage(data);

            OnMessageUpdate?.Invoke(this, new MessageUpdateEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_DELETE")]
        void HandleMessageDeleteEvent(JsonElement data)
        {
            Snowflake messageId = data.GetProperty("id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnMessageDelete?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
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

                OnMessageDelete?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
            }
        }

        [DispatchEvent("MESSAGE_REACTION_ADD")]
        void HandleMessageReactionAddEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();
            JsonElement emojiData = data.GetProperty("emoji");

            var emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionAdd?.Invoke(this, new MessageReactionAddEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE")]
        void HandleMessageReactionRemoveEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();
            JsonElement emojiData = data.GetProperty("emoji");

            var emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionRemove?.Invoke(this, new MessageReactionRemoveEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE_ALL")]
        void HandleMessageReactionRemoveAllEvent(JsonElement data)
        {
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            Snowflake messageId = data.GetProperty("message_id").GetSnowflake();

            OnMessageReactionRemoveAll?.Invoke(this, new MessageReactionRemoveAllEventArgs(shard, messageId, channelId));
        }
        #endregion

        [DispatchEvent("WEBHOOKS_UPDATE")]
        void HandleWebhooksUpdate(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();

            OnWebhookUpdate?.Invoke(this, new WebhooksUpdateEventArgs(shard, guildId, channelId));
        }

        [DispatchEvent("PRESENCE_UPDATE")]
        void HandlePresenceUpdateEvent(JsonElement data)
        {
            Snowflake guildId = data.GetProperty("guild_id").GetSnowflake();

            var presence = new DiscordUserPresence(data, guildId);

            // Fire event
            OnPresenceUpdate?.Invoke(this, new PresenceUpdateEventArgs(shard, guildId, presence));
        }

        [DispatchEvent("TYPING_START")]
        void HandleTypingStartEvent(JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            Snowflake channelId = data.GetProperty("channel_id").GetSnowflake();
            int timestamp = data.GetProperty("timestamp").GetInt32();

            OnTypingStart?.Invoke(this, new TypingStartEventArgs(shard, userId, channelId, timestamp));
        }

        [DispatchEvent("USER_UPDATE")]
        void HandleUserUpdateEvent(JsonElement data)
        {
            var user = new DiscordUser(data, isWebhookUser: false);

            OnUserUpdate?.Invoke(this, new UserUpdateEventArgs(shard, user));
        }

        #region Voice
        [DispatchEvent("VOICE_STATE_UPDATE")]
        async Task HandleVoiceStateUpdateEvent(JsonElement data)
        {
            Snowflake? guildId = data.GetPropertyOrNull("guild_id")?.GetSnowflake();
            if (guildId.HasValue) // Only guild voice channels are supported.
            {
                Snowflake userId = data.GetProperty("user_id").GetSnowflake();

                // Update the voice state
                var voiceState = new DiscordVoiceState(data, guildId: guildId.Value);

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
                OnVoiceStateUpdate?.Invoke(this, new VoiceStateUpdateEventArgs(shard, voiceState));
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
                string? endpoint = data.GetProperty("endpoint").GetString();

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
