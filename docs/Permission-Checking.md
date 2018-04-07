[[← back]](./README.md)

# Permission Checking

Discore provides a helper class for calculating whether a user has a certain permission. This is available through `DiscordPermissionHelper`.

### Example: `!delete` Command
If you are creating a public bot, this can be used for example to prevent users without the `ManageMessages` permission from using a bot command that can mass-delete messages.

```csharp
private static void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
{
    DiscordMessage message = e.Message;
    DiscordShardCache cache = e.Shard.Cache;

    // Note: This example assumes that this message originated from a guild channel, 
    // and that everything is available in the cache.
    // Applications should take care in obtaining this information safely.

    DiscordGuildTextChannel guildTextChannel = cache.GetGuildTextChannel(message.ChannelId);
    DiscordGuild guild = cache.GetGuild(guildTextChannel.GuildId);
    DiscordGuildMember member = cache.GetGuildMember(guild.Id, message.Author.Id);

    if (message.Content.StartsWith("!delete"))
    {
        // Check if this user has permission to use this command in this channel.
        if (DiscordPermissionHelper.HasPermission(DiscordPermission.ManageMessages, 
            member, guild, guildTextChannel))
        {
            // Handle !delete command...
        }
    }
}
```