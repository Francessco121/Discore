using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoreTest
{
    public static class TestHelper
    {
        public static void Assert(bool condition, string errorMessage)
        {
            if (!condition)
                throw new Exception(errorMessage);
        }
    }
}
