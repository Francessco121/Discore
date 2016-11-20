using System.Collections.Generic;

namespace Discore.WebSocket
{
    public interface ITextChannel
    {
        DiscordMessage SendMessage(string content, bool tts = false);
        DiscordMessage SendMessage(string content, byte[] fileAttachment, bool tts = false);
        bool BulkDeleteMessages(IEnumerable<Snowflake> messageIds);

        bool TriggerTypingIndicator();

        DiscordMessage GetMessage(Snowflake messageId);
        IList<DiscordMessage> GetMessages(Snowflake? baseMessageId = null, int? limit = null, 
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before);
        IList<DiscordMessage> GetPinnedMessages();

        bool Delete();
    }
}
