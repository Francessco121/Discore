using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild news channel.
    /// </summary>
    public class GuildNewsChannelOptions
    {
        /// <summary>
        /// Gets or sets the name of the channel (or null to leave unchanged).
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the sorting position of the channel (or null to leave unchanged).
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the topic of the news channel (or null to leave unchanged).
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Gets or sets whether this news channel is NSFW (not-safe-for-work) (or null to leave unchanged).
        /// </summary>
        public bool? Nsfw { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent category channel (or null to leave unchanged).
        /// <para>Note: Set to <see cref="Snowflake.None"/> to clear the parent ID.</para>
        /// </summary>
        public Snowflake? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the list of permission overwrites (or null to leave unchanged).
        /// </summary>
        public IList<OverwriteOptions>? PermissionOverwrites { get; set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        public GuildNewsChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildNewsChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Sets the topic of the news channel.
        /// </summary>
        public GuildNewsChannelOptions SetTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        /// <summary>
        /// Sets whether this news channel is NSFW (not-safe-for-work).
        /// </summary>
        public GuildNewsChannelOptions SetNsfw(bool nsfw)
        {
            Nsfw = nsfw;
            return this;
        }

        /// <summary>
        /// Sets the ID of the parent category channel.
        /// </summary>
        /// <param name="parentId">
        /// The ID of the category to use as a parent or <see cref="Snowflake.None"/> to clear the parent ID.
        /// </param>
        public GuildNewsChannelOptions SetParentId(Snowflake parentId)
        {
            ParentId = parentId;
            return this;
        }

        /// <summary>
        /// Sets the list of permission overwrites.
        /// </summary>
        public GuildNewsChannelOptions SetPermissionOverwrites(IList<OverwriteOptions> permissionOverwrites)
        {
            PermissionOverwrites = permissionOverwrites;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Name != null)
                writer.WriteString("name", Name);
            if (Position.HasValue)
                writer.WriteNumber("position", Position.Value);
            if (Topic != null)
                writer.WriteString("topic", Topic);
            if (Nsfw.HasValue)
                writer.WriteBoolean("nsfw", Nsfw.Value);

            if (ParentId.HasValue)
            {
                if (ParentId.Value == Snowflake.None)
                    writer.WriteSnowflake("parent_id", null);
                else
                    writer.WriteSnowflake("parent_id", ParentId.Value);
            }

            if (PermissionOverwrites != null)
            {
                writer.WriteStartArray("permission_overwrites");

                foreach (OverwriteOptions overwriteParam in PermissionOverwrites)
                    overwriteParam.Build(writer);

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}

#nullable restore
