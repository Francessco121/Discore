using System;

namespace Discore
{
    static class TimeHelper
    {
        /// <summary>
        /// Checks if Environment.TickCount is equal to or has passed the specified number of ticks.
        /// Will handle the fact that Environment.TickCount wraps around after ~24 days.
        /// </summary>
        public static bool HasTickCountHit(int ticks)
        {
            int tickCount = Environment.TickCount;
            if (tickCount >= 0 && ticks >= 0)
                return tickCount >= ticks;
            else if (tickCount < 0 && ticks >= 0)
                return true;
            else if (tickCount >= 0 && ticks < 0)
                return false;
            else if (tickCount < 0 && ticks < 0)
                return tickCount >= ticks;

            return false;
        }
    }
}
