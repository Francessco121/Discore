using Discore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoreTest.Tests
{
    [TestClass]
    public class SnowflakeTest
    {
        [TestMethod]
        public static void TestBitwise()
        {
            Snowflake id = new Snowflake(175928847299117063);

            DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1462015105796);
            DateTimeOffset extractedTimestamp = id.GetTimestamp();
            TestHelper.Assert(extractedTimestamp == timestamp, "Failed to extract timestamp.");

            int workerId = id.GetWorkerId();
            TestHelper.Assert(workerId == 1, "Failed to extract worker id.");

            int processId = id.GetProcessId();
            TestHelper.Assert(processId == 0, "Failed to extract process id.");

            int increment = id.GetIncrement();
            TestHelper.Assert(increment == 7, "Failed to extract increment.");
        }
    }
}
