namespace Discore.Http
{
    /// <summary>
    /// Error codes returned by the discord HTTP API.
    /// https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/topics/RESPONSE_CODES.md#json-error-response
    /// </summary>
    public enum DiscordHttpErrorCode
    {
        /// <summary>
        /// An unknown error occured, this most likely signifies that an internal error occured on Discord's end.
        /// </summary>
        None = 0,

        /// <summary>
        /// The request failed because the application was rate limited.
        /// </summary>
        TooManyRequests = 429,

        UnknownAccount = 10001,
        UnknownApplication,
        UnknownChannel,
        UnknownGuild,
        UnknownIntegration,
        UnknownInvite,
        UnknownMember,
        UnknownMessage,
        UnknownOverwrite,
        UnknownProvider,
        UnknownRole,
        UnknownToken,
        UnknownUser,
        UnknownEmoji,
        /// <summary>
        /// Reason: Bots cannot use this endpoint.
        /// </summary>
        BotsNotAllowed = 20001,
        /// <summary>
        /// Reason: Only bots can use this endpoint.
        /// </summary>
        OnlyBotsAllowed,
        /// <summary>
        /// Maximum guilds: 100
        /// </summary>
        MaximumGuildsReached = 30001,
        /// <summary>
        /// Maximum pinned messages: 50
        /// </summary>
        MaximumPinsReached,
        /// <summary>
        /// Maximum guild roles: 250
        /// </summary>
        MaximumGuildRolesReached = 30005,
        TooManyReactions = 30010,
        Unauthorized = 40001,
        MissingAccess = 50001,
        InvalidAccountType,
        /// <summary>
        /// Reason: Cannot execute action on a DM channel.
        /// </summary>
        InvalidDMChannelAction,
        EmbedDisabled,
        /// <summary>
        /// Reason: Cannot edit a message created by a different user.
        /// </summary>
        InvalidMessageAuthorEdit,
        /// <summary>
        /// Reason: Cannot send an empty message.
        /// </summary>
        MessageEmpty,
        /// <summary>
        /// Reason: Cannot send messages to this user.
        /// </summary>
        CannotMessageUser,
        /// <summary>
        /// Reason: Cannot send message in a voice channel.
        /// </summary>
        CannotMessageVoiceChannel,
        /// <summary>
        /// Reason: Channel verification level is too high.
        /// </summary>
        ChannelVerificationError,
        /// <summary>
        /// Reason: OAuth2 application does not have a bot.
        /// </summary>
        OAuth2AppMissingBot,
        /// <summary>
        /// Reason: OAuth2 application limit reached.
        /// </summary>
        OAuth2AppLimitReached,
        InvalidOAuthState,
        MissingPermissions,
        InvalidAuthenticationToken,
        /// <summary>
        /// Reason: Note is too long.
        /// </summary>
        NoteTooLong,
        /// <summary>
        /// Reason: Provided too few or too many messages to delete.
        /// <para>Must provide at least 2 and fewer than 100 messages to delete.</para>
        /// </summary>
        InvalidBulkDelete,
        /// <summary>
        /// Reason: A message can only be pinned to the channel it was created in.
        /// </summary>
        InvalidMessagePin = 50019,
        /// <summary>
        /// Reason: Cannot execute action on a system message.
        /// </summary>
        InvalidMessageTarget = 50021,
        /// <summary>
        /// Reason: A message provided was too old to bulk delete.
        /// </summary>
        InvalidBulkDeleteMessageAge = 50034,
        InvalidFormBody = 50035,
        ReactionBlocked = 90001
    }
}