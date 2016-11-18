using System.Collections.Generic;

namespace Discore.Http
{
    public class DiscordConnection : DiscordIdObject
    {
        /// <summary>
        /// Gets the username of the connection account.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the service of the connection (twitch, youtube, etc.).
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets whether this connection has been revoked.
        /// </summary>
        public bool IsRevoked { get; }

        /// <summary>
        /// Gets a list of partial integrations associated with this connection.
        /// </summary>
        public DiscordIntegration[] Integrations { get; }

        public DiscordConnection(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            Type = data.GetString("type");
            IsRevoked = data.GetBoolean("revoked").Value;

            IList<DiscordApiData> integrationsData = data.GetArray("integrations");
            Integrations = new DiscordIntegration[integrationsData.Count];

            for (int i = 0; i < integrationsData.Count; i++)
                Integrations[i] = new DiscordIntegration(integrationsData[i]);
        }
    }
}
