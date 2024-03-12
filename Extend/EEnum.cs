using System;
using System.Collections.Generic;

namespace Cherry.Extend
{
    public static class EEnum
    {
        public static string[] ToEnumStrings(this Type type)
        {
            var values = new List<string>();
            foreach (var item in Enum.GetValues(type)) values.Add(item.ToString());

            return values.ToArray();
        }

        public static bool TestMask(this Enum val, Enum mask)
        {
            return ((1 << (int)(object)val) & (int)(object)mask) > 0;
        }

        public static bool TestMask(this int val, int mask)
        {
            return ((1 << val) & mask) > 0;
        }
    }
}