using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore.Http
{
    public class AllowedMentionsOptions
    {
        /// <summary>
        /// Gets or sets the list of allowed mention types to parse from the content.
        /// </summary>
        public IList<AllowedMentionType>? Parse { get; set; }

        /// <summary>
        /// Gets or sets the list of role IDs to mention (max of 100).
        /// </summary>
        public IList<Snowflake>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of user IDs to mention (max of 100).
        /// </summary>
        public IList<Snowflake>? Users { get; set; }

        /// <summary>
        /// Gets or sets, for replies, whether to mention the author of the message being replied to (default false).
        /// </summary>
        public bool? RepliedUser { get; set; }

        /// <summary>
        /// Sets the list of allowed mention types to parse from the content.
        /// </summary>
        public AllowedMentionsOptions SetParse(IList<AllowedMentionType> parse)
        {
            Parse = parse;
            return this;
        }

        /// <summary>
        /// Sets the list of role IDs to mention (max of 100).
        /// </summary>
        public AllowedMentionsOptions SetRoles(IList<Snowflake> roles)
        {
            Roles = roles;
            return this;
        }

        /// <summary>
        /// Sets the list of user IDs to mention (max of 100).
        /// </summary>
        public AllowedMentionsOptions SetUsers(IList<Snowflake> users)
        {
            Users = users;
            return this;
        }

        /// <summary>
        /// Sets, for replies, whether to mention the author of the message being replied to.
        /// </summary>
        public AllowedMentionsOptions SetRepliedUser(bool? repliedUser)
        {
            RepliedUser = repliedUser;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Parse != null)
            {
                writer.WriteStartArray("parse");

                foreach (AllowedMentionType type in Parse)
                    writer.WriteStringValue(MentionTypeToString(type));

                writer.WriteEndArray();
            }

            if (Roles != null)
            {
                writer.WriteStartArray("roles");

                foreach (Snowflake roleId in Roles)
                    writer.WriteSnowflakeValue(roleId);

                writer.WriteEndArray();
            }

            if (Users != null)
            {
                writer.WriteStartArray("users");

                foreach (Snowflake userId in Users)
                    writer.WriteSnowflakeValue(userId);

                writer.WriteEndArray();
            }

            if (RepliedUser != null)
            {
                writer.WriteBoolean("replied_user", RepliedUser.Value);
            }

            writer.WriteEndObject();
        }

        static string MentionTypeToString(AllowedMentionType type)
        {
            return type switch
            {
                AllowedMentionType.Users => "users",
                AllowedMentionType.Roles => "roles",
                AllowedMentionType.Everyone => "everyone",
                _ => throw new NotImplementedException($"Unsupported allowed mention type: {type}"),
            };
        }
    }
}
