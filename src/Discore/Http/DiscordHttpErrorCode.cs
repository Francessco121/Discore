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
        UnknownWebhook,
        UnknownWebhookService,
        UnknownSession = 10020,
        UnknownBan = 10026,
        UnknownSku,
        UnknownStoreListing,
        UnknownEntitlement,
        UnknownBuild,
        UnknownLobby,
        UnknownBranch,
        UnknownStoreDirectoryLayout,
        UnknownRedistributable = 10036,
        UnknownGiftCode = 10038,
        UnknownGuildTemplate = 10057,
        UnknownInteraction = 10062,
        UnknownApplicationCommand,
        UnknownApplicationCommandPermissions = 10066,
        UnknownStageInstance,

        /// <summary>
        /// Bots cannot use this endpoint.
        /// </summary>
        BotsNotAllowed = 20001,
        /// <summary>
        /// Only bots can use this endpoint.
        /// </summary>
        OnlyBotsAllowed,
        /// <summary>
        /// Explicit content cannot be sent to the desired recipient(s).
        /// </summary>
        ExplicitContentCannotBeSent = 20009,
        /// <summary>
        /// You are not authorized to perform this action on this application.
        /// </summary>
        UnauthorizedForApplication = 20012,
        /// <summary>
        /// This action cannot be performed due to slowmode rate limit.
        /// </summary>
        SlowmodeRateLimited = 20016,
        /// <summary>
        /// Only the owner of this account can perform this action.
        /// </summary>
        OwnershipRequired = 20018,
        /// <summary>
        /// This message cannot be edited due to announcement rate limits.
        /// </summary>
        AccountmentRateLimited = 20022,
        /// <summary>
        /// The channel you are writing has hit the write rate limit.
        /// </summary>
        ChannelRateLimited = 20028,
        /// <summary>
        /// Your Stage topic, server name, server description, or channel names contain words that are not allowed.
        /// </summary>
        DisallowedWords = 20031,
        /// <summary>
        /// Guild premium subscription level too low.
        /// </summary>
        PremiumSubscriptionTooLow = 20035,

        /// <summary>
        /// Maximum guilds: 100
        /// </summary>
        MaximumGuildsReached = 30001,
        /// <summary>
        /// Maximum pinned messages: 50
        /// </summary>
        MaximumPinsReached = 30003,
        /// <summary>
        /// Maximum recipients: 10
        /// </summary>
        MaximumRecipientsReached = 30004,
        /// <summary>
        /// Maximum guild roles: 250
        /// </summary>
        MaximumGuildRolesReached,
        /// <summary>
        /// Maximum webhooks: 10
        /// </summary>
        MaximumWebhooksReached = 30007,
        MaximumEmojisReached,
        /// <summary>
        /// Maximum reactions: 20
        /// </summary>
        MaximumReactionsReached = 30010,
        /// <summary>
        /// Maximum guild channels: 500
        /// </summary>
        MaximumGuildChannelsReached = 30013,
        /// <summary>
        /// Maximum message attachments: 10
        /// </summary>
        MaximumAttachmentsReached = 30015,
        /// <summary>
        /// Maximum invites: 1000
        /// </summary>
        MaximumInvitesReached,
        MaximumAnimatedEmojisReached = 30018,
        MaximumServerMembersReached,
        GuildAlreadyHasTemplate = 30031,
        MaximumThreadParticipantsReached = 30033,
        /// <summary>
        /// Maximum number of bans for non-guild members have been exceeded.
        /// </summary>
        MaximumBansExceeded = 30035,
        MaximumBanFetchesReached = 30037,

        /// <summary>
        /// Unauthorized. Provide a valid token and try again.
        /// </summary>
        Unauthorized = 40001,
        /// <summary>
        /// You need to verify your account in order to perform this action.
        /// </summary>
        VerifiedAccountRequired,
        /// <summary>
        /// You are opening direct messages too fast.
        /// </summary>
        OpeningDMsTooFast,
        /// <summary>
        /// Request entity too large. Try sending something smaller in size.
        /// </summary>
        RequestEntityTooLarge = 40005,
        /// <summary>
        /// This feature has been temporarily disabled server-side.
        /// </summary>
        DisabledFeature,
        /// <summary>
        /// The user is banned from this guild.
        /// </summary>
        UserBanned,
        /// <summary>
        /// Target user is not connected to voice.
        /// </summary>
        TargetNotInVoice = 40032,
        /// <summary>
        /// This message has already been crossposted.
        /// </summary>
        MessageAlreadyCrossposted,
        /// <summary>
        /// An application command with that name already exists.
        /// </summary>
        DuplicateApplicationCommand,

        MissingAccess = 50001,
        InvalidAccountType,
        /// <summary>
        /// Cannot execute action on a DM channel.
        /// </summary>
        InvalidDMChannelAction,
        WidgetDisabled,
        /// <summary>
        /// Cannot edit a message created by a different user.
        /// </summary>
        InvalidMessageAuthorEdit,
        /// <summary>
        /// Cannot send an empty message.
        /// </summary>
        MessageEmpty,
        /// <summary>
        /// Cannot send messages to this user.
        /// </summary>
        CannotMessageUser,
        /// <summary>
        /// Cannot send message in a voice channel.
        /// </summary>
        CannotMessageVoiceChannel,
        /// <summary>
        /// Channel verification level is too high.
        /// </summary>
        ChannelVerificationError,
        /// <summary>
        /// OAuth2 application does not have a bot.
        /// </summary>
        OAuth2AppMissingBot,
        /// <summary>
        /// OAuth2 application limit reached.
        /// </summary>
        OAuth2AppLimitReached,
        InvalidOAuthState,
        MissingPermissions,
        InvalidAuthenticationToken,
        /// <summary>
        /// Note is too long.
        /// </summary>
        NoteTooLong,
        /// <summary>
        /// Provided too few or too many messages to delete.
        /// <para>Must provide at least 2 and fewer than 100 messages to delete.</para>
        /// </summary>
        InvalidBulkDelete,
        /// <summary>
        /// A message can only be pinned to the channel it was created in.
        /// </summary>
        InvalidMessagePin = 50019,
        /// <summary>
        /// Invite code was either invalid or taken.
        /// </summary>
        InvalidInviteCode,
        /// <summary>
        /// Cannot execute action on a system message.
        /// </summary>
        InvalidMessageTarget,
        /// <summary>
        /// Cannot execute action on this channel type.
        /// </summary>
        InvalidChannelTarget = 50024,
        /// <summary>
        /// Invalid OAuth2 access token provided.
        /// </summary>
        InvalidAccessToken,
        /// <summary>
        /// Missing required OAuth2 scope.
        /// </summary>
        MissingOAuth2Scope,
        /// <summary>
        /// Invalid webhook token provided.
        /// </summary>
        InvalidWebhookToken,
        InvalidRole,
        InvalidRecipients = 50033,
        /// <summary>
        /// A message provided was too old to bulk delete.
        /// </summary>
        InvalidBulkDeleteMessageAge,
        InvalidFormBody,
        /// <summary>
        /// An invite was accepted to a guild the application's bot is not in.
        /// </summary>
        InviteAcceptedToInvalidGuild,
        /// <summary>
        /// Cannot delete a channel required for Community guilds.
        /// </summary>
        CannotDeleteCommunityRequiredChannel = 50074,
        /// <summary>
        /// Invalid sticker sent.
        /// </summary>
        InvalidSticker = 50081,
        /// <summary>
        /// Tried to perform an operation on an archived thread, such as editing a message or adding a user to the thread.
        /// </summary>
        ThreadArchived = 50083,
        InvalidThreadNotificationSettings,
        /// <summary>
        /// 'before' value is earlier than the thread creation date.
        /// </summary>
        InvalidThreadBefore,

        TwoFactorRequired = 60003,

        /// <summary>
        /// No users with DiscordTag exist.
        /// </summary>
        DiscordTagNotFound = 80004,

        ReactionBlocked = 90001,

        /// <summary>
        /// API resource is currently overloaded. Try again a little later.
        /// </summary>
        ResourceOverloaded = 130000,

        StageAlreadyOpen = 150006,

        /// <summary>
        /// A thread has already been created for this message.
        /// </summary>
        ThreadAlreadyCreated = 160004,
        ThreadLocked,
        MaximumActiveThreadsReached,
        MaximumActiveAnnouncementThreadsReached
    }
}
