using System;
using System.Collections.Generic;
using System.Linq;

namespace Discore.Http
{
    public class AllowedMentionsOptions
    {
        /// <summary>
        /// Gets or sets the list of allowed mention types to parse from the content.
        /// </summary>
        public IList<AllowedMentionType> Parse { get; set; }

        /// <summary>
        /// Gets or sets the list of role IDs to mention (max of 100).
        /// </summary>
        public IList<Snowflake> Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of user IDs to mention (max of 100).
        /// </summary>
        public IList<Snowflake> Users { get; set; }

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

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            if (Parse != null)
                data.Set("parse", Parse.Select(p => MentionTypeToString(p)).ToArray());
            if (Roles != null)
                data.Set("roles", Roles);
            if (Users != null)
                data.Set("users", Users);
            if (RepliedUser != null)
                data.Set("replied_user", RepliedUser.Value);

            return data;
        }

        static string MentionTypeToString(AllowedMentionType type)
        {
            switch (type)
            {
                case AllowedMentionType.Users:
                    return "users";
                case AllowedMentionType.Roles:
                    return "roles";
                case AllowedMentionType.Everyone:
                    return "everyone";
                default:
                    throw new NotImplementedException($"Unsupported allowed mention type: {type}");
            }
        }
    }
}
