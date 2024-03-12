using System;
using UnityEngine;

namespace Cherry.Extend
{
    public static class EFloat
    {
        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;

        /// <summary>
        ///     计算文件大小例如:100MB
        /// </summary>
        /// <param name="size"></param>
        /// <param name="roundCount"></param>
        /// <returns></returns>
        public static string FormatSize(this long size, int roundCount = 2)
        {
            if (KBCount > size) return $"{size}B";

            if (MBCount > size) return $"{Math.Round(size / KBCount, roundCount, MidpointRounding.AwayFromZero)}KB";

            if (GBCount > size) return $"{Math.Round(size / MBCount, roundCount, MidpointRounding.AwayFromZero)}MB";

            if (TBCount > size) return $"{Math.Round(size / GBCount, roundCount, MidpointRounding.AwayFromZero)}GB";

            return $"{Math.Round(size / TBCount, roundCount, MidpointRounding.AwayFromZero)}TB";
        }

        /// <summary>
        ///     按步进值四舍五入
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static float Round(this float val, float step)
        {
            return Mathf.RoundToInt(val / step) * step;
        }

        /// <summary>
        ///     按步进值取最小整
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static float Ceil(this float val, float step)
        {
            return Mathf.CeilToInt(val / step) * step;
        }

        /// <summary>
        ///     按步进值取取大整
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static float Floor(this float val, float step)
        {
            return Mathf.FloorToInt(val / step) * step;
        }
    }
}