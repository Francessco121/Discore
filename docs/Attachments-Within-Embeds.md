[[← back]](./README.md)

# Attachments Within Embeds

When uploading files as attachments via the `UploadFile` methods, these attachments can be used within embeds for the same message. When this is done, the attachment will not show up twice, instead it will essentially just be moved.

To do this, simply pass the string `"attachment://file_name.ext"` to `DiscordEmbedBuilder.SetImage` where `file_name.ext` is the exact name used when calling `UploadFile`.

For more information on this feature [see the Discord documentation](https://discordapp.com/developers/docs/resources/channel#using-attachments-within-embeds).

## Example
```csharp
Stream attachment = ...;
await textChannel.UploadFile(attachment, "fileName.jpg", new DiscordMessageDetails()
    .SetEmbed(new DiscordEmbedBuilder()
        .SetImage("attachment://fileName.jpg")
        .SetTitle("Embed Title")));
```