using System;
using System.Collections.Generic;

namespace Cherry.Extend
{
    public static class EDictionary
    {
        public static void Every<KT, VT>(this Dictionary<KT, VT> dic, Action<KT, VT> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext()) action(e.Current.Key, e.Current.Value);
            }
        }

        public static void Every<KT, VT>(this Dictionary<KT, VT> dic, Action<VT> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext()) action(e.Current.Value);
            }
        }

        public static void Every<KT, VT>(this Dictionary<KT, VT> dic, Action<KT> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext()) action(e.Current.Key);
            }
        }

        public static bool Any<KT, VT>(this Dictionary<KT, VT> dic, Func<KT, VT, bool> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext())
                    if (action(e.Current.Key, e.Current.Value))
                        return true;
            }

            return false;
        }

        public static bool Any<KT, VT>(this Dictionary<KT, VT> dic, Func<VT, bool> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext())
                    if (action(e.Current.Value))
                        return true;
            }

            return false;
        }

        public static bool Any<KT, VT>(this Dictionary<KT, VT> dic, Func<KT, bool> action)
        {
            using (var e = dic.GetEnumerator())
            {
                while (e.MoveNext())
                    if (action(e.Current.Key))
                        return true;
            }

            return false;
        }
    }
}