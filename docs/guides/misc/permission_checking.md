# Permission Checking
Discore provides a helper class for calculating whether a user has a certain set of permissions. This is available through [`DiscordPermissionHelper`](xref:Discore.DiscordPermissionHelper).

Utilities include:
- [Checking if a guild member has permissions in a channel](xref:Discore.DiscordPermissionHelper.HasPermission(Discore.DiscordPermission,Discore.IDiscordGuildMember,Discore.DiscordGuild,Discore.DiscordGuildChannel)).
- [Checking if a guild member can join a voice channel](xref:Discore.DiscordPermissionHelper.CanJoinVoiceChannel*).
- [Converting `DiscordPermission`s to a string for debugging](xref:Discore.DiscordPermissionHelper.PermissionsToString*).

## Example: `!delete` Command
If you are creating a public bot, this can be used, for example, to prevent users without the [`ManageMessages`](xref:Discore.DiscordPermission.ManageMessages) permission from using a bot command that can mass-delete messages.

```csharp
void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
{
    DiscordMessage message = e.Message;

    if (message.Member == null)
    {
        // Ignore DMs.
        return;
    }

    // Obtain the full entities for the channel and guild.
    //
    // This is necessary because only the full entity classes contain role
    // permissions and channel-specific permission overwrites.
    //
    // Typically, this is achieved by using a DiscordMemoryCache or by caching
    // guilds and guild channels in a custom way.
    DiscordGuildChannel guildChannel = ...;
    DiscordGuild guild = ...;

    if (message.Content.StartsWith("!delete"))
    {
        // Check if this user has permission to use this command in this channel.
        //
        // TIP: You can also call message.Member.HasPermission(...), which is a
        // shortcut for DiscordPermissionHelper.HasPermission.
        if (DiscordPermissionHelper.HasPermission(DiscordPermission.ManageMessages, 
            message.Member, guild, guildChannel))
        {
            // Handle !delete command...
        }
    }
}
```
