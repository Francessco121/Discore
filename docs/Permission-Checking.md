[[← back]](./README.md)

# Permission Checking

Discore provides a helper class for calculating whether a user has a certain permission. This is available through `DiscordPermissionHelper`.

### Example: `!delete` Command
If you are creating a public bot, this can be used for example to prevent users without the `ManageMessages` permission from using a bot command that can mass-delete messages.

```csharp
private static void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
{
    DiscordMessage message = e.Message;
    DiscoreCache cache = e.Shard.Cache;

    // Get the text channel the message was sent in, from cache and expect it to be a guild channel.
    DiscordGuildTextChannel guildTextChannel = cache.Channels.Get(message.ChannelId) as DiscordGuildTextChannel;
    if (guildTextChannel != null)
    {
        // Get the cache of the guild the channel is in.
        DiscoreGuildCache guildCache = cache.Guilds.Get(guildTextChannel.GuildId);

        // Get the cache of the member who sent this message.
        DiscoreMemberCache memberCache = guildCache.Members.Get(message.Author.Id);

        // Note: You will most likely want to check if guildCache and memberCache were retrieved successfully.

        if (message.Content.StartsWith("!delete"))
        {
            // Check if this user has permission to use this command in this channel.
            if (DiscordPermissionHelper.HasPermission(DiscordPermission.ManageMessages, memberCache.Value, guildCache.Value, guildTextChannel))
            {
                // Handle !delete command...
            }
        }
    }
}
```

For information on how to subscribe to this event, [see the sharding documentation](./Sharding.md#gateway-interaction).