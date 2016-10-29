using Discore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoreTest.Tests
{
    [TestClass]
    public class ApiCacheTest
    {
        [TestMethod]
        public static void Test()
        {
            DiscordApiCache cache = new DiscordApiCache();

            DiscordApiData userData = DiscordApiDataSamples.GetDiscordUser();
            DiscordUser user = cache.Set(userData, userData.GetString("id"), () => new DiscordUser());

            DiscordApiData guildMemberData = DiscordApiDataSamples.GetDiscordGuildMember();
            DiscordGuildMember member = cache.Set(guildMemberData, guildMemberData.LocateString("user.id"), () => new DiscordGuildMember(cache));

            TestHelper.Assert(member.User == user, "Member.user was not retrieved");
        }

        [TestMethod]
        public static void TestSnapshotSaftey()
        {
            DiscordApiCache cache = new DiscordApiCache();

            DiscordApiData userData = DiscordApiDataSamples.GetDiscordUser();

            userData.Set("discriminator", "0000");
            DiscordUser snapshotA = cache.Set(userData, userData.GetString("id"), () => new DiscordUser());

            userData.Set("discriminator", "1111");
            DiscordUser snapshotB = cache.Set(userData, userData.GetString("id"), () => new DiscordUser());

            TestHelper.Assert(!ReferenceEquals(snapshotA, snapshotB), "References should not be equal");
            TestHelper.Assert(snapshotA.Discriminator != snapshotB.Discriminator, "Discriminators should not be equal");
        }
    }
}
