using System.Threading.Tasks;

namespace Discore.Net
{
    /// <summary>
    /// A <see cref="IDiscordRestClient"/> service for interacting with <see cref="DiscordChannel"/>s.
    /// </summary>
    public interface IDiscordRestChannelsService
    {
        /// <summary>
        /// Gets a <see cref="DiscordGuildChannel"/> or <see cref="DiscordDMChannel"/> by its id.
        /// </summary>
        /// <param name="channelId">The id of the <see cref="DiscordChannel"/>.</param>
        /// <param name="type">The type of <see cref="DiscordChannel"/> to return.</param>
        /// <returns>Returns the specified <see cref="DiscordChannel"/>.</returns>
        Task<DiscordChannel> Get(string channelId, DiscordChannelType type);

        /// <summary>
        /// Updates a <see cref="DiscordGuildChannel"/>'s settings.
        /// </summary>
        /// <param name="guildChannel">The <see cref="DiscordGuildChannel"/> to update.</param>
        /// <param name="settings">The settings to change.</param>
        /// <returns>Returns the updated <see cref="DiscordGuildChannel"/>.</returns>
        Task<DiscordGuildChannel> Modify(DiscordGuildChannel guildChannel, DiscordGuildChannelModifyParams settings);

        /// <summary>
        /// Deletes a <see cref="DiscordGuildChannel"/> or closes a <see cref="DiscordDMChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to delete/close.</param>
        /// <returns>Returns the deleted/closed <see cref="DiscordChannel"/>.</returns>
        Task<DiscordChannel> Close(DiscordChannel channel);
    }
}
