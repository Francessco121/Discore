# Embed Attachments
When uploading files as attachments via the [`CreateMessage`](xref:Discore.Http.DiscordHttpClient.CreateMessage*) methods, these attachments can be used in embeds within same message. When this is done, the attachment will not show up twice. Instead, it will essentially just be moved inside of the embed.

To do this, simply pass the string `"attachment://file_name.ext"` to [`EmbedOptions.SetImage`](xref:Discore.Http.EmbedOptions.SetImage*) where `file_name.ext` is the exact name passed to [`AttachmentOptions.SetFileName`](xref:Discore.Http.AttachmentOptions.SetFileName*).

For more information on this feature [see the Discord documentation](https://discord.com/developers/docs/reference#editing-message-attachments-using-attachments-within-embeds).

## Example
```csharp
byte[] file = ...;

await httpClient.CreateMessage(channelId, new CreateMessageOptions()
    .AddAttachment(new AttachmentOptions(0)
        .SetFileName("filename.png")
        .SetContent(file))
    .AddEmbed(new EmbedOptions()
        .SetImage("attachment://filename.png")));
```
