﻿using System;

namespace Discore
{
    public class DiscordIntegration : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool Enabled { get; private set; }
        public bool Syncing { get; private set; }
        public string RoleId { get; private set; }
        public int ExpireBehavior { get; private set; }
        public int ExpireGracePeriod { get; private set; }
        public DiscordUser User { get; private set; }
        public DiscordIntegrationAccount Account { get; private set; }
        public DateTime SyncedAt { get; private set; }

        DiscordApiCache cache;

        public DiscordIntegration(IDiscordClient client)
        {
            cache = client.Cache;
        }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            Type = data.GetString("type") ?? Type;
            Enabled = data.GetBoolean("enabled") ?? Enabled;
            Syncing = data.GetBoolean("syncing") ?? Syncing;
            RoleId = data.GetString("role_id") ?? RoleId; // TODO: grab actual role
            ExpireBehavior = data.GetInteger("expire_behavior") ?? ExpireBehavior;
            ExpireGracePeriod = data.GetInteger("expire_grace_period") ?? ExpireGracePeriod;
            SyncedAt = data.GetDateTime("synced_at") ?? SyncedAt;

            DiscordApiData userData = data.Get("user");
            if (userData != null)
            {
                string userId = userData.GetString("id");
                User = cache.AddOrUpdate(userId, userData, () => { return new DiscordUser(); });
            }

            DiscordApiData accountData = data.Get("account");
            if (accountData != null)
            {
                if (Account == null)
                    Account = new DiscordIntegrationAccount();

                Account.Update(accountData);
            }
        }
    }
}
