namespace Discore.Http
{
    /// <summary>
    /// Error codes returned by the discord HTTP API.
    /// https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/topics/RESPONSE_CODES.md#json-error-response
    /// </summary>
    public enum DiscordHttpErrorCode
    {
        None = 0,

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
        BotsCannotUseThisEndpoint = 20001,
        OnlyBotsCanUseThisEndpoint,
        /// <summary>
        /// Maximum Guilds: 100
        /// </summary>
        MaximumGuildsReached = 30001,
        /// <summary>
        /// Maximum Friends: 1000
        /// </summary>
        MaximumFriendsReached,
        /// <summary>
        /// Maximum Pinned Messages: 50
        /// </summary>
        MaximumPinsReached,
        /// <summary>
        /// Maximum Guild Roles: 250
        /// </summary>
        MaximumGuildRolesReached = 30005,
        TooManyReactions = 30010,
        Unauthorized = 40001,
        MissingAccess = 50001,
        InvalidAccountType,
        CannotExecuteActionOnDMChannel,
        EmbedDisabled,
        CannotEditMessageByOtherUser,
        CannotSendEmptyMessage,
        CannotSendMessagesToUser,
        CannotSendMessagesInVoiceChannel,
        ChannelVerificationLevelTooHigh,
        OAuth2ApplicationDoesNotHaveBot,
        OAuth2ApplicationLimitReached,
        InvalidOAuthState,
        MissingPermissions,
        InvalidAuthenticationToken,
        NoteIsTooLong,
        /// <summary>
        /// Must provide at least 2 and fewer than 100 messages to delete.
        /// </summary>
        ProvidedTooFewOrTooManyMessagesToDelete,
        MessagesCanOnlyBePinnedInTheChannelItWasCreated = 50019,
        AMessageProvidedWasTooOldToBulkDelete = 50034,
        ReactionBlocked = 90001
    }
}