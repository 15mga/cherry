using System;
using System.Text;

namespace Cherry.Extend
{
    public static class EBit
    {
        public static bool[] ToBits(this byte val)
        {
            var vals = new bool[8];
            for (var i = 0; i < 8; i++) vals[i] = (val & (byte)(1 << i)) > 0;

            return vals;
        }

        public static byte ToByte(this bool[] bools)
        {
            if (bools.Length != 8) throw new ArgumentException(nameof(bools));
            byte val = 0;
            for (var i = 0; i < 8; i++)
                if (bools[i])
                    val |= (byte)(1 << i);
                else
                    val &= (byte)~(1 << i);

            return val;
        }

        public static string ToBitStr(this bool[] bools)
        {
            var str = new StringBuilder();
            for (var i = 0; i < bools.Length; i++)
            {
                str.Append(bools[i] ? "1" : "0");

                if (i > 0 && i % 8 == 0) str.Append(" ");
            }

            return str.ToString();
        }

        public static string ToBitStr(this int n)
        {
            if (n == 0) return "0";
            var r = "";
            for (; n > 0; n /= 2) r = n + r;

            return r;
        }
    }
}