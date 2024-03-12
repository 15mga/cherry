using System;

namespace Cherry.Misc
{
    public static class Utils
    {
        public static int TestTime(Action action)
        {
            var start = DateTime.Now;
            action();
            return DateTime.Now.Subtract(start).Milliseconds;
        }
    }
}