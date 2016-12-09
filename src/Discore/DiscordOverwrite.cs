using Discore.Http.Net;
using System;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public sealed class DiscordOverwrite : DiscordIdObject
    {
        public Snowflake ChannelId { get; }

        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordOverwrite(IDiscordApplication app, Snowflake channelId, DiscordApiData data)
            : base(data)
        {
            channelsHttp = app.HttpApi.InternalApi.Channels;

            ChannelId = channelId;

            string typeStr = data.GetString("type");
            DiscordOverwriteType type;
            if (Enum.TryParse(typeStr, true, out type))
                Type = type;

            long allow = data.GetInt64("allow").Value;
            Allow = (DiscordPermission)allow;

            long deny = data.GetInt64("deny").Value;
            Deny = (DiscordPermission)deny;
        }

        /// <summary>
        /// Edits the permissions of this overwrite.
        /// If successful, changes will be immediately reflected for this instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public bool Edit(DiscordPermission allow, DiscordPermission deny)
        {
            try { return EditAsync(allow, deny).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Edits the permissions of this overwrite.
        /// If successful, changes will be immediately reflected for this instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public async Task<bool> EditAsync(DiscordPermission allow, DiscordPermission deny)
        {
            DiscordApiData data = await channelsHttp.EditPermissions(ChannelId, Id, allow, deny, Type);
            return data.IsNull;
        }

        /// <summary>
        /// Deletes this overwrite.
        /// If successful, changes will be immediately reflected for the channel this overwrite was in.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public bool Delete()
        {
            try { return DeleteAsync().Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes this overwrite.
        /// If successful, changes will be immediately reflected for the channel this overwrite was in.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public async Task<bool> DeleteAsync()
        {
            DiscordApiData data = await channelsHttp.DeletePermission(ChannelId, Id);
            return data.IsNull;
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
