using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cherry.Misc;
using UnityEngine;
using UnityEngine.Networking;

// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.NamingConventions;

namespace Cherry.Extend
{
    public static class EString
    {
        public static readonly Dictionary<string, AudioType> AudioTypes = new()
        {
            { ".acc", AudioType.ACC },
            { ".aiff", AudioType.AIFF },
            { ".it", AudioType.IT },
            { ".mod", AudioType.MOD },
            { ".mp3", AudioType.MPEG },
            { ".ogg", AudioType.OGGVORBIS },
            { ".s3m", AudioType.S3M },
            { ".wav", AudioType.WAV },
            { ".xm", AudioType.XM },
            { ".xma", AudioType.XMA },
            { ".vag", AudioType.VAG }
        };

        /// <summary>
        ///     按指定长度填补字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fill"></param>
        /// <param name="len"></param>
        /// <param name="pre"></param>
        /// <returns></returns>
        public static string FillStr(this string str, string fill = "0", int len = 2, bool pre = true)
        {
            while (str.Length < len)
                if (pre)
                    str = fill + str;
                else
                    str += fill;

            return str;
        }

        /// <summary>
        ///     十六进制字符串转字节数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToBytes(this string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0) hex += " ";

            var len = hex.Length / 2;
            var bytes = new byte[len];
            for (var i = 0; i < len; i++) bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }

        /// <summary>
        ///     UTF8字符串转二进制数组
        /// </summary>
        /// <param name="utf8"></param>
        /// <returns></returns>
        public static byte[] ToUTF8Bytes(this string utf8)
        {
            return Encoding.UTF8.GetBytes(utf8);
        }

        /// <summary>
        ///     字符串转枚举
        /// </summary>
        /// <param name="str"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public static int ParseInt(this string str)
        {
            return int.TryParse(str.Trim(' '), out var val) ? val : 0;
        }

        public static float ParseFloat(this string str)
        {
            return float.TryParse(str.Trim(' '), out var val) ? val : 0;
        }

        /// <summary>
        ///     字符串转为数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        /// <param name="sep"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object[] ParseArray<T>(this string str, Func<string, T> func,
            char startChar = '[', char endChar = ']', char sep = ',')
        {
            str = str.Replace("\r\n", "\n").Replace("\n", "");
            var start = 0;
            var listStack = new Stack<List<object>>();
            List<object> list = null;
            var total = str.Length;
            var canAddItem = true;

            void AddItem(int i)
            {
                if (!canAddItem) return;
                var len = i - start;
                var s = str.Substring(start, len).Trim();
                start = i + 1;
                if (string.IsNullOrEmpty(s)) return;
                list.Add(func(s));
            }

            for (var i = 0; i < total; i++)
            {
                var c = str[i];
                if (c == startChar)
                {
                    canAddItem = true;
                    start = i + 1;
                    list = new List<object>();
                    if (listStack.Count > 0) listStack.First().Add(list);
                    listStack.Push(list);
                }
                else if (c == sep)
                {
                    AddItem(i);
                }
                else if (c == endChar)
                {
                    AddItem(i);
                    canAddItem = false;
                    if (listStack.Count > 1)
                    {
                        listStack.Pop();
                        list = listStack.First();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list.ToArray();
        }

        /// <summary>
        ///     字符串转为数组
        ///     [1, 2, 3]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ParseArr<T>(this string str, Func<string, T> func = null, char startChar = '[',
            char endChar = ']',
            char sep = ',')
        {
            var arr1 = str.ParseArray(func, startChar, endChar, sep);
            var list1 = new List<T>();
            foreach (var item1 in arr1) list1.Add((T)item1);

            return list1.ToArray();
        }

        /// <summary>
        ///     字符串转为二维数组
        ///     [[1, 2, 3], [4, 5, 6]]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[][] ParseArr2<T>(this string str, Func<string, T> func, char startChar = '[',
            char endChar = ']', char sep = ',')
        {
            var arr1 = str.ParseArray(func, startChar, endChar, sep);

            return (from List<object> arr2 in arr1
                    select arr2.Cast<T>()
                        .ToArray())
                .ToArray();
        }

        /// <summary>
        ///     字符串转为三维数组
        ///     [[[1, 2, 3], [4, 5, 6]], [[1, 2, 3], [4, 5, 6]]]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[][][] ParseArr3<T>(this string str, Func<string, T> func, char startChar = '[',
            char endChar = ']', char sep = ',')
        {
            var arr1 = str.ParseArray(func, startChar, endChar, sep);

            return (from List<object> arr2 in arr1
                    select (from List<object> arr3 in arr2
                            select arr3.Cast<T>()
                                .ToArray())
                        .ToArray())
                .ToArray();
        }

        /// <summary>
        ///     字符串转为四维数组
        ///     [[[[1, 2, 3], [4, 5, 6]], [[7, 8, 9], [10, 11, 12]]], [[[13, 14, 15], [16, 17, 18]], [[19, 20, 21], [22, 23, 24]]]]
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        /// <param name="sep"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[][][][] ParseArr4<T>(this string str, Func<string, T> func,
            char startChar = '[', char endChar = ']', char sep = ',')
        {
            var arr1 = str.ParseArray(func, startChar, endChar, sep);

            return (from List<object> arr2 in arr1
                    select (from List<object> arr3 in arr2
                            select (from List<object> arr4 in arr3
                                    select arr4.Cast<T>()
                                        .ToArray())
                                .ToArray())
                        .ToArray())
                .ToArray();
        }

        /// <summary>
        ///     {a:1, b:{c:2, d:3}}
        /// </summary>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        /// <param name="itemSep"></param>
        /// <param name="valSep"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<KT, object> ParseMap<KT, VT>(this string str, Func<string, KT> kFunc,
            Func<KT, string, VT> vFunc, char startChar = '{', char endChar = '}', char itemSep = ',', char valSep = ':')
        {
            str = str.Replace("\r\n", "\n").Replace("\n", "");
            var start = 0;
            var dicStack = new Stack<Dictionary<KT, object>>();
            KT key = default;
            Dictionary<KT, object> dic = null;
            var total = str.Length;
            var canAddItem = true;

            void AddValue(int i)
            {
                if (!canAddItem) return;
                var s = str.Substring(start, i - start).Trim();
                start = i + 1;
                if (string.IsNullOrEmpty(s)) return;
                dic.Add(key, vFunc(key, s));
            }

            for (var i = 0; i < total; i++)
            {
                var c = str[i];
                if (c == startChar)
                {
                    start = i + 1;
                    dic = new Dictionary<KT, object>();
                    if (dicStack.Count > 0) dicStack.First().Add(key, dic);
                    dicStack.Push(dic);
                }
                else if (c == valSep)
                {
                    canAddItem = true;
                    key = kFunc(str.Substring(start, i - start).Trim());
                    start = i + 1;
                }
                else if (c == itemSep)
                {
                    if (!canAddItem) start = i + 1;
                    AddValue(i);
                }
                else if (c == endChar)
                {
                    AddValue(i);
                    canAddItem = false;
                    if (dicStack.Count > 1)
                    {
                        dicStack.Pop();
                        dic = dicStack.First();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return dic;
        }

        public static Dictionary<KT, T> ParseDic<KT, T>(this string str, Func<string, KT> kFunc,
            Func<string, T> vFunc, char startChar = '{', char endChar = '}', char itemSep = ',', char valSep = ':')
        {
            var res = new Dictionary<KT, T>();
            var dic = str.ParseMap(kFunc, (k, v) => vFunc(v), startChar, endChar, itemSep, valSep);
            foreach (var item in dic) res.Add(item.Key, (T)item.Value);

            return res;
        }

        public static Dictionary<string, T> ParseDic<T>(this string str, Func<string, T> vFunc,
            Func<string, string> kFunc = null, char startChar = '{', char endChar = '}', char itemSep = ',',
            char valSep = ':')
        {
            var res = new Dictionary<string, T>();
            var dic = str.ParseMap(kFunc ?? (s => s), (k, v) => vFunc(v), startChar, endChar, itemSep, valSep);
            foreach (var item in dic) res.Add(item.Key, (T)item.Value);

            return res;
        }

        public static Dictionary<KT, Dictionary<KT, T>> ParseDic2<KT, T>(this string str,
            Func<string, KT> kFunc, Func<string, T> vFunc, char startChar = '{', char endChar = '}',
            char itemSep = ',', char valSep = ':')
        {
            var res = new Dictionary<KT, Dictionary<KT, T>>();
            var dic1 = str.ParseMap(kFunc, (k, v) => vFunc(v), startChar, endChar, itemSep, valSep);
            foreach (var item in dic1)
            {
                var dic2 = (Dictionary<KT, object>)item.Value;
                var map2 = new Dictionary<KT, T>();
                res.Add(item.Key, map2);
                foreach (var item2 in dic2) map2.Add(item2.Key, (T)item2.Value);
            }

            return res;
        }

        public static Dictionary<string, Dictionary<string, T>> ParseDic2<T>(this string str,
            Func<string, T> vFunc, Func<string, string> kFunc = null, char startChar = '{', char endChar = '}',
            char itemSep = ',', char valSep = ':')
        {
            var res = new Dictionary<string, Dictionary<string, T>>();
            var dic1 = str.ParseMap(kFunc ?? (s => s), (k, v) => vFunc(v), startChar, endChar, itemSep, valSep);
            foreach (var item in dic1)
            {
                var dic2 = (Dictionary<string, object>)item.Value;
                var map2 = new Dictionary<string, T>();
                res.Add(item.Key, map2);
                foreach (var item2 in dic2) map2.Add(item2.Key, (T)item2.Value);
            }

            return res;
        }

        public static IEnumerator LoadWebTexture2D(this string url, Action<Texture2D, IError> onComplete)
        {
            var c = loadWebTexture(url, onComplete);
            Game.StartCo(c);
            return c;
        }

        private static IEnumerator loadWebTexture(string url, Action<Texture2D, IError> onComplete)
        {
            using var req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Game.Log.Error(req.error);
                onComplete(null, new Error(req.error));
            }
            else
            {
                onComplete(DownloadHandlerTexture.GetContent(req), null);
            }
        }

        public static IEnumerator LoadWebAudio(this string url, Action<AudioClip, IError> onComplete)
        {
            var c = loadWebAudio(url, onComplete);
            Game.StartCo(c);
            return c;
        }

        private static IEnumerator loadWebAudio(string url, Action<AudioClip, IError> onComplete)
        {
            if (!AudioTypes.TryGetValue(Path.GetExtension(url), out var audioType))
                audioType = AudioType.UNKNOWN;
            using var req = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Game.Log.Error($"load web audio {url} failed: {req.error}");
                onComplete(null, new Error(req.error));
            }
            else
            {
                onComplete(DownloadHandlerAudioClip.GetContent(req), null);
            }
        }

        public static IEnumerator LoadWebTxt(this string url, Action<string, IError> onComplete)
        {
            var c = loadWebTxt(url, onComplete);
            Game.StartCo(c);
            return c;
        }

        private static IEnumerator loadWebTxt(string url, Action<string, IError> onComplete)
        {
            using var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Game.Log.Error(req.error);
                onComplete(null, new Error(req.error));
            }
            else
            {
                onComplete(req.downloadHandler.data.ToUTF8(), null);
            }
        }

        public static IEnumerator LoadWebBytes(this string url, Action<byte[], IError> onComplete)
        {
            var c = loadWebBytes(url, onComplete);
            Game.StartCo(c);
            return c;
        }

        private static IEnumerator loadWebBytes(string url, Action<byte[], IError> onComplete)
        {
            using var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Game.Log.Error(req.error);
                onComplete(null, new Error(req.error));
            }
            else
            {
                onComplete(req.downloadHandler.data, null);
            }
        }
    }
}