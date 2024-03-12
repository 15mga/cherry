using UnityEngine;

namespace Cherry.Extend
{
    public static class EVector3
    {
        /// <summary>
        ///     按步进值四舍五入
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static Vector3 Round(this Vector3 val, float step)
        {
            return new Vector3(val.x.Round(step), val.y.Round(step), val.z.Round(step));
        }

        /// <summary>
        ///     按步进值取小整
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static Vector3 Ceil(this Vector3 val, float step)
        {
            return new Vector3(val.x.Ceil(step), val.y.Ceil(step), val.z.Ceil(step));
        }

        /// <summary>
        ///     按步进值取大整
        /// </summary>
        /// <param name="val"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static Vector3 Floor(this Vector3 val, float step)
        {
            return new Vector3(val.x.Floor(step), val.y.Floor(step), val.z.Floor(step));
        }
    }
}