using System.Threading.Tasks;

namespace Discore.Net
{
    /// <summary>
    /// A <see cref="IDiscordRestClient"/> service for interacting with <see cref="DiscordUser"/>s.
    /// </summary>
    public interface IDiscordRestUsersService
    {
        /// <summary>
        /// Opens a DM channel with a user.
        /// </summary>
        /// <param name="recipientId">The user to open the DM with.</param>
        /// <returns>Returns the created DM channel.</returns>
        Task<DiscordDMChannel> CreateDM(string recipientId);
    }
}