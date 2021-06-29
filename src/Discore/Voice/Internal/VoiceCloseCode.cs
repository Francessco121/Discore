namespace Discore.Voice.Internal
{
    enum VoiceCloseCode
    {
        UnknownOpCode = 4001,
        NotAuthenticated = 4003,
        AuthenticationFailed = 4004,
        AlreadyAuthenticated = 4005,
        InvalidSession = 4006,
        SessionTimeout = 4009,
        ServerNotFound = 4011,
        UnknownProtocol = 4012,
        Disconnected = 4014,
        VoiceServerCrashed = 4015,
        UnknownEncryptionMode = 4016
    }
}
