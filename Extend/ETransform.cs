using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cherry.Extend
{
    public static class ETransform
    {
        /// <summary>
        ///     通过名称获取子节点,优先同级节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Transform FindTnf(this Transform parent, string name, bool recursive = true)
        {
            return new List<Transform>(parent.GetComponentsInChildren<Transform>()).FindTnf(name, recursive);
        }

        /// <summary>
        ///     通过名称获取子节点,优先同级节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static RectTransform FindRTnf(this Transform parent, string name, bool recursive = true)
        {
            return parent.FindComp<RectTransform>(name, recursive);
        }

        /// <summary>
        ///     通过名称获取子对象，优先同级节点
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="tranforms"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Transform FindTnf(this IEnumerable<Transform> tranforms, string name, bool recursive = true)
        {
            while (true)
            {
                if (!recursive)
                {
                    return tranforms.FirstOrDefault(tnf => tnf.name == name);
                }

                var list = new List<Transform>();
                foreach (var tnf in tranforms)
                {
                    if (tnf.name == name) return tnf;
                    if (tnf.childCount == 0) continue;
                    for (var i = 0; i < tnf.childCount; i++)
                    {
                        var child = tnf.GetChild(i);
                        list.Add(child);
                    }
                }

                if (list.Count == 0) return null;
                tranforms = list;
            }
        }

        /// <summary>
        ///     通过名称获取指定名称子对象的组件
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static T FindComp<T>(this Transform parent, string name, bool recursive = true)
        {
            var obj = parent.FindTnf(name, recursive);
            return obj == null ? default : obj.GetComponent<T>();
        }

        public static Component FindComp(this Transform parent, Type type, string name, bool recursive = true)
        {
            var obj = parent.FindTnf(name, recursive);
            return obj == null ? null : obj.GetComponent(type);
        }

        /// <summary>
        ///     是否任意子对象满足条件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter"></param>
        /// <param name="recursive"></param>
        public static bool AnyChild(this Transform target, Predicate<Transform> filter, bool recursive = true)
        {
            var length = target.childCount;
            for (var i = 0; i < length; i++)
            {
                var child = target.GetChild(i);

                if (filter(child)) return true;
                if (recursive) child.AnyChild(filter, recursive);
            }

            return false;
        }

        /// <summary>
        ///     遍历所有子对象
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter"></param>
        /// <param name="recursive"></param>
        public static void EveryChild(this Transform target, Action<Transform> filter, bool recursive = true)
        {
            var length = target.childCount;
            for (var i = 0; i < length; i++)
            {
                var child = target.GetChild(i);

                filter(child);
                if (recursive) child.EveryChild(filter, recursive);
            }
        }

        /// <summary>
        ///     是否所有子对象满足条件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filter"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static bool EveryChild(this Transform target, Func<Transform, bool> filter, bool recursive = true)
        {
            var length = target.childCount;
            for (var i = 0; i < length; i++)
            {
                var child = target.GetChild(i);

                if (!filter(child)) return false;
                if (recursive) child.EveryChild(filter, recursive);
            }

            return true;
        }

        /// <summary>
        ///     获取父对象组件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static T FindParentComponent<T>(this Transform obj, bool recursive = true)
        {
            if (obj == null) return default;

            var current = obj;

            do
            {
                var parent = current.GetComponent<T>();
                if (parent != null) return parent;
                if (recursive)
                    current = current.parent;
                else
                    break;
            } while (current != null);

            return default;
        }

        /// <summary>
        ///     获取子节点的路径
        /// </summary>
        /// <param name="tnf"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static string PathTo(this Transform tnf, Transform parent)
        {
            var tempParent = tnf.parent;
            var path = "";
            while (true)
            {
                if (tempParent == null) return null;

                if (tempParent == parent) return path.Length > 0 ? path.Substring(0, path.Length - 1) : path;

                path = $"{tempParent.name}/{path}";

                tempParent = tempParent.parent;
            }
        }

        /// <summary>
        ///     获取节点路径
        /// </summary>
        /// <param name="tnf"></param>
        /// <returns></returns>
        public static string Path(this Transform tnf)
        {
            var parent = tnf.parent;
            var path = "";
            while (parent != null)
            {
                path = $"{parent.name}/{path}";

                parent = parent.parent;
            }

            return path;
        }

        /// <summary>
        ///     通过路径子节点
        /// </summary>
        /// <param name="tnf"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Transform FindWithPath(this Transform tnf, string path)
        {
            if (path == "") return tnf;

            var arr = path.Split('/');
            var tmpTnf = tnf;
            for (var index = 0; index < arr.Length; index++)
            {
                var name = arr[index];
                tmpTnf = tmpTnf.FindTnf(name);
                if (tmpTnf == null) return null;
            }

            return tmpTnf;
        }

        /// <summary>
        ///     相对位置
        /// </summary>
        /// <param name="tnf"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector3 RelativePositionTo(this Transform tnf, Transform target)
        {
            return tnf.position - target.position;
        }
    }
}