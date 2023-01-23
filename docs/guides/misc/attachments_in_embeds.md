# Attachments Within Embeds
When uploading files as attachments via the `CreateMessage` methods, these attachments can be used within embeds for the same message. When this is done, the attachment will not show up twice, instead it will essentially just be moved inside the embed.

To do this, simply pass the string `"attachment://file_name.ext"` to `EmbedOptions.SetImage` where `file_name.ext` is the exact name used when calling `CreateMessage`.

For more information on this feature [see the Discord documentation](https://discord.com/developers/docs/resources/channel#create-message-using-attachments-within-embeds).

## Example
```csharp
Stream attachment = ...;

await httpClient.CreateMessage(channelId, attachment, "fileName.jpg", new CreateMessageOptions()
    .SetEmbed(new EmbedOptions()
        .SetImage("attachment://fileName.jpg")
    )
);
```