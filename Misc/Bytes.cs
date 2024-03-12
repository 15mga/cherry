using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Cherry.Misc
{
    /// <summary>
    ///     如果用于对象池,获取后请记得Reset
    /// </summary>
    public class Bytes : IDisposable
    {
        /// <summary>
        ///     标记的位置
        /// </summary>
        private int _markPos;

        public Bytes() : this(512)
        {
            Pos = 0;

            Length = 0;

            Buffer = new byte[Cap];
        }

        public Bytes(int cap)
        {
            Pos = 0;

            Length = 0;

            Buffer = new byte[cap];
        }

        public Bytes(byte[] bs)
        {
            Pos = 0;

            Length = bs.Length;

            Buffer = bs;
        }

        /// <summary>
        ///     缓冲数组
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        ///     读取/写入的当前位置
        /// </summary>
        public int Pos { get; set; }

        /// <summary>
        ///     缓冲长度
        /// </summary>
        public int Cap => Buffer.Length;

        /// <summary>
        ///     有效长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        ///     剩余有效长度
        /// </summary>
        public int Available => Length - Pos;

        /// <summary>
        ///     释放
        /// </summary>
        public void Dispose()
        {
            Reset();
            Buffer = null;
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            Pos = 0;
            Length = 0;
            _markPos = 0;
        }

        /// <summary>
        ///     标记当前位置
        /// </summary>
        public void Mark()
        {
            _markPos = Pos;
        }

        /// <summary>
        ///     重置位置到标记的位置
        /// </summary>
        public void ResetToMark()
        {
            Pos = _markPos;
        }

        /// <summary>
        ///     将当前位置转为整型指针
        /// </summary>
        /// <returns></returns>
        public IntPtr PosIntPtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(Buffer, Pos);
        }

        /// <summary>
        ///     复制有效长度
        /// </summary>
        /// <returns></returns>
        public byte[] All()
        {
            var newBytes = new byte[Length];
            Array.Copy(Buffer, 0, newBytes, 0, Length);
            return newBytes;
        }

        /// <summary>
        ///     写入流
        /// </summary>
        /// <param name="stream"></param>
        public void CopyTo(Stream stream)
        {
            stream.Write(Buffer, 0, Length);
        }

        /// <summary>
        ///     复制到字节数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public int CopyTo(byte[] bytes, int offset = 0)
        {
            Array.Copy(Buffer, 0, bytes, offset, Length);
            return Length;
        }

        /// <summary>
        ///     写入
        /// </summary>
        /// <param name="ba"></param>
        public void Write(Bytes ba)
        {
            Write(ba.Buffer, 0, ba.Length);
        }

        /// <summary>
        ///     移除无效的缓冲字节
        /// </summary>
        public void RemoveUnavailable()
        {
            var newBytes = new byte[Available];
            Array.Copy(Buffer, Pos, newBytes, 0, Available);
            _markPos -= Pos;
            Length = Available;
            Buffer = newBytes;
            Pos = 0;
        }

        private void _CheckCap(int l)
        {
            if (Pos + l <= Cap) return;

            var newBytes = new byte[Pos + (l > Pos ? l : Pos)];
            Array.Copy(Buffer, 0, newBytes, 0, Pos);
            Buffer = newBytes;
        }

        /// <summary>
        ///     读字节
        /// </summary>
        /// <returns></returns>
        public byte RByte()
        {
            var b = Buffer[Pos];
            Pos++;
            return b;
        }

        public byte[] RBytes()
        {
            return RBytes(RUInt16());
        }

        /// <summary>
        ///     读字节数组
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public byte[] RBytes(int l)
        {
            var bs = new byte[l];
            Array.Copy(Buffer, Pos, bs, 0, l);
            Pos += l;
            return bs;
        }

        /// <summary>
        ///     写字节
        /// </summary>
        /// <param name="v"></param>
        public void Write(byte v)
        {
            _CheckCap(1);
            Buffer[Pos] = v;
            Pos++;
            Length = Pos;
        }

        /// <summary>
        ///     写字节数组
        /// </summary>
        /// <param name="v"></param>
        public void Write(byte[] v)
        {
            Write(v, 0, v.Length);
        }

        /// <summary>
        ///     写字节数组
        /// </summary>
        /// <param name="v"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public void Write(byte[] v, int offset, int len)
        {
            _CheckCap(len);
            Array.Copy(v, offset, Buffer, Pos, len);
            Pos += len;
            Length = Pos;
        }

        /// <summary>
        ///     读布尔
        /// </summary>
        /// <returns></returns>
        public bool RBool()
        {
            return RByte() == 1;
        }

        public bool[] RBools()
        {
            var l = RUInt16();
            var bools = new bool[l];
            for (var i = 0; i < l; i++) bools[i] = RBool();

            return bools;
        }

        /// <summary>
        ///     写布尔
        /// </summary>
        /// <param name="v"></param>
        public void Write(bool v)
        {
            if (v)
                Write((byte)1);
            else
                Write((byte)0);
        }

        public void Write(bool[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        private byte[] _REndianBytes(int l)
        {
            var bs = RBytes(l);
            if (BitConverter.IsLittleEndian) Array.Reverse(bs);

            return bs;
        }

        private void _WEndianBytes(byte[] bs)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(bs);

            Write(bs);
        }

        /// <summary>
        ///     读有效的字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] RAvailable()
        {
            return RBytes(Available);
        }

        /// <summary>
        ///     读无符号短整型
        /// </summary>
        /// <returns></returns>
        public ushort RUInt16()
        {
            return BitConverter.ToUInt16(_REndianBytes(2), 0);
        }

        /// <summary>
        ///     读无符号短整型数组
        /// </summary>
        /// <returns></returns>
        public ushort[] RUInt16s()
        {
            var len = (int)RUInt16();
            var a = new ushort[len];
            for (var i = 0; i < len; i++) a[i] = RUInt16();

            return a;
        }

        /// <summary>
        ///     写无符号短整型
        /// </summary>
        /// <param name="v"></param>
        public void Write(ushort v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        /// <summary>
        ///     写无符号短整型数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(ushort[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读短整型
        /// </summary>
        /// <returns></returns>
        public short RInt16()
        {
            return BitConverter.ToInt16(_REndianBytes(2), 0);
        }

        /// <summary>
        ///     读无符号短整型数组
        /// </summary>
        /// <returns></returns>
        public short[] RInt16s()
        {
            var len = (int)RUInt16();
            var a = new short[len];
            for (var i = 0; i < len; i++) a[i] = RInt16();

            return a;
        }

        /// <summary>
        ///     写短整型
        /// </summary>
        /// <param name="v"></param>
        public void Write(short v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        /// <summary>
        ///     写无符号短整型数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(short[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读无符号32位整型
        /// </summary>
        /// <returns></returns>
        public uint RUInt32()
        {
            return BitConverter.ToUInt32(_REndianBytes(4), 0);
        }

        public uint[] RUInt32s()
        {
            var len = (int)RUInt16();
            var a = new uint[len];
            for (var i = 0; i < len; i++) a[i] = RUInt32();

            return a;
        }

        /// <summary>
        ///     写无符号32位整型
        /// </summary>
        /// <returns></returns>
        public void Write(uint v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(uint[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读32位整型
        /// </summary>
        /// <returns></returns>
        public int RInt()
        {
            return BitConverter.ToInt32(_REndianBytes(4), 0);
        }

        public int[] RInts()
        {
            var len = (int)RUInt16();
            var a = new int[len];
            for (var i = 0; i < len; i++) a[i] = RInt();
            return a;
        }

        public int[][] RInts2()
        {
            var len = (int)RUInt16();
            var a = new int[len][];
            for (var i = 0; i < len; i++) a[i] = RInts();
            return a;
        }

        public int[][][] RInts3()
        {
            var len = (int)RUInt16();
            var a = new int[len][][];
            for (var i = 0; i < len; i++) a[i] = RInts2();
            return a;
        }

        public int[][][][] RInts4()
        {
            var len = (int)RUInt16();
            var a = new int[len][][][];
            for (var i = 0; i < len; i++) a[i] = RInts3();
            return a;
        }

        /// <summary>
        ///     写32位整型
        /// </summary>
        /// <returns></returns>
        public void Write(int v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(int[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(int[][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(int[][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(int[][][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读无符号长整型
        /// </summary>
        /// <returns></returns>
        public ulong RUInt64()
        {
            return BitConverter.ToUInt64(_REndianBytes(8), 0);
        }

        public ulong[] RUInt64s()
        {
            var len = (int)RUInt16();
            var a = new ulong[len];
            for (var i = 0; i < len; i++) a[i] = RUInt64();

            return a;
        }

        /// <summary>
        ///     写无符号长整型
        /// </summary>
        /// <param name="v"></param>
        public void Write(ulong v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(ulong[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读长整型
        /// </summary>
        /// <returns></returns>
        public long RInt64()
        {
            return BitConverter.ToInt64(_REndianBytes(8), 0);
        }

        public long[] RInt64s()
        {
            var len = (int)RUInt16();
            var a = new long[len];
            for (var i = 0; i < len; i++) a[i] = RInt64();

            return a;
        }

        /// <summary>
        ///     写长整型
        /// </summary>
        /// <param name="v"></param>
        public void Write(long v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(long[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读32位浮点
        /// </summary>
        /// <returns></returns>
        public float RFloat()
        {
            return BitConverter.ToSingle(_REndianBytes(4), 0);
        }

        public float[] RFloats()
        {
            var len = (int)RUInt16();
            var a = new float[len];
            for (var i = 0; i < len; i++) a[i] = RFloat();

            return a;
        }

        public float[][] RFloats2()
        {
            var len = (int)RUInt16();
            var a = new float[len][];
            for (var i = 0; i < len; i++) a[i] = RFloats();
            return a;
        }

        public float[][][] RFloats3()
        {
            var len = (int)RUInt16();
            var a = new float[len][][];
            for (var i = 0; i < len; i++) a[i] = RFloats2();
            return a;
        }

        public float[][][][] RFloats4()
        {
            var len = (int)RUInt16();
            var a = new float[len][][][];
            for (var i = 0; i < len; i++) a[i] = RFloats3();
            return a;
        }

        /// <summary>
        ///     写32位浮点
        /// </summary>
        /// <param name="v"></param>
        public void Write(float v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(float[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(float[][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(float[][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(float[][][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读64位浮点
        /// </summary>
        /// <returns></returns>
        public double RDouble()
        {
            return BitConverter.ToDouble(_REndianBytes(8), 0);
        }

        public double[] RDoubles()
        {
            var len = (int)RUInt16();
            var a = new double[len];
            for (var i = 0; i < len; i++) a[i] = RDouble();

            return a;
        }

        /// <summary>
        ///     写64位浮点
        /// </summary>
        /// <param name="v"></param>
        public void Write(double v)
        {
            _WEndianBytes(BitConverter.GetBytes(v));
        }

        public void Write(double[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读2位长度开始的utf8字符串
        /// </summary>
        /// <returns></returns>
        public string RString()
        {
            return Encoding.UTF8.GetString(RBytes(RUInt16()));
        }

        public string[] RStrings()
        {
            var len = (int)RUInt16();
            var a = new string[len];
            for (var i = 0; i < len; i++) a[i] = RString();

            return a;
        }

        public string[][] RStrings2()
        {
            var len = (int)RUInt16();
            var a = new string[len][];
            for (var i = 0; i < len; i++) a[i] = RStrings();
            return a;
        }

        public string[][][] RStrings3()
        {
            var len = (int)RUInt16();
            var a = new string[len][][];
            for (var i = 0; i < len; i++) a[i] = RStrings2();
            return a;
        }

        public string[][][][] RStrings4()
        {
            var len = (int)RUInt16();
            var a = new string[len][][][];
            for (var i = 0; i < len; i++) a[i] = RStrings3();
            return a;
        }

        /// <summary>
        ///     将剩余有效字节都按utf8字符串读取
        /// </summary>
        /// <returns></returns>
        public string RAvailableString()
        {
            return Encoding.UTF8.GetString(RBytes(Available));
        }

        /// <summary>
        ///     写2位长度开始的utf8字符串
        /// </summary>
        /// <param name="v"></param>
        public void Write(string v)
        {
            var bs = Encoding.UTF8.GetBytes(v);
            Write((ushort)bs.Length);
            Write(bs);
        }

        /// <summary>
        ///     写utf8字符串数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(string[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(string[][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(string[][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        public void Write(string[][][][] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     写utf8字符串
        /// </summary>
        /// <returns></returns>
        public void WriteNoHeader(string v)
        {
            Write(Encoding.UTF8.GetBytes(v));
        }

        /// <summary>
        ///     读2位长度开始的unicode字符串
        /// </summary>
        /// <returns></returns>
        public string RUnicode()
        {
            return Encoding.Unicode.GetString(RBytes(RUInt16()));
        }

        /// <summary>
        ///     写2位长度开始的unicode字符串
        /// </summary>
        /// <param name="v"></param>
        public void WUnicode(string v)
        {
            var bs = Encoding.Unicode.GetBytes(v);
            Write((ushort)bs.Length);
            Write(bs);
        }

        public Dictionary<string, bool> RMapBool()
        {
            var dic = new Dictionary<string, bool>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RBool());

            return dic;
        }

        public void Write(Dictionary<string, bool> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, Dictionary<string, bool>> RMapBool2()
        {
            var dic = new Dictionary<string, Dictionary<string, bool>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RMapBool());

            return dic;
        }

        public void Write(Dictionary<string, Dictionary<string, bool>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, string> RMap()
        {
            var dic = new Dictionary<string, string>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RString());

            return dic;
        }

        public void Write(Dictionary<string, string> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, Dictionary<string, string>> RMap2()
        {
            var dic = new Dictionary<string, Dictionary<string, string>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RMap());

            return dic;
        }

        public void Write(Dictionary<string, Dictionary<string, string>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, int> RMapInt()
        {
            var dic = new Dictionary<string, int>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RInt());

            return dic;
        }

        public void Write(Dictionary<string, int> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, Dictionary<string, int>> RMapInt2()
        {
            var dic = new Dictionary<string, Dictionary<string, int>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RMapInt());

            return dic;
        }

        public void Write(Dictionary<string, Dictionary<string, int>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, float> RMapFlo()
        {
            var dic = new Dictionary<string, float>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RFloat());

            return dic;
        }

        public void Write(Dictionary<string, float> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<string, Dictionary<string, float>> RMapFlo2()
        {
            var dic = new Dictionary<string, Dictionary<string, float>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RString(), RMapFlo());

            return dic;
        }

        public void Write(Dictionary<string, Dictionary<string, float>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, bool> RIntMapBool()
        {
            var dic = new Dictionary<int, bool>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RBool());

            return dic;
        }

        public void Write(Dictionary<int, bool> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, Dictionary<int, bool>> RIntMapBool2()
        {
            var dic = new Dictionary<int, Dictionary<int, bool>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RIntMapBool());

            return dic;
        }

        public void Write(Dictionary<int, Dictionary<int, bool>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, int> RIntMapInt()
        {
            var dic = new Dictionary<int, int>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RInt());

            return dic;
        }

        public void Write(Dictionary<int, int> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, Dictionary<int, int>> RIntMapInt2()
        {
            var dic = new Dictionary<int, Dictionary<int, int>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RIntMapInt());

            return dic;
        }

        public void Write(Dictionary<int, Dictionary<int, int>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, float> RIntMapFlo()
        {
            var dic = new Dictionary<int, float>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RFloat());

            return dic;
        }

        public void Write(Dictionary<int, float> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, Dictionary<int, float>> RIntMapFlo2()
        {
            var dic = new Dictionary<int, Dictionary<int, float>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RIntMapFlo());

            return dic;
        }

        public void Write(Dictionary<int, Dictionary<int, float>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, string> RIntMapStr()
        {
            var dic = new Dictionary<int, string>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RString());

            return dic;
        }

        public void Write(Dictionary<int, string> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        public Dictionary<int, Dictionary<int, string>> RIntMapStr2()
        {
            var dic = new Dictionary<int, Dictionary<int, string>>();
            var l = RUInt16();
            for (var i = 0; i < l; i++) dic.Add(RInt(), RIntMapStr());

            return dic;
        }

        public void Write(Dictionary<int, Dictionary<int, string>> map)
        {
            Write((ushort)map.Count);
            foreach (var item in map)
            {
                Write(item.Key);
                Write(item.Value);
            }
        }

        /// <summary>
        ///     读Vector2
        /// </summary>
        /// <returns></returns>
        public Vector2 RVector2()
        {
            return new Vector2
            {
                x = RFloat(),
                y = RFloat()
            };
        }

        /// <summary>
        ///     写Vector2
        /// </summary>
        /// <param name="vec"></param>
        public void Write(Vector2 vec)
        {
            Write(vec.x);
            Write(vec.y);
        }

        /// <summary>
        ///     读Vector2数组
        /// </summary>
        /// <returns></returns>
        public Vector2[] RVector2s()
        {
            var len = (int)RUInt16();
            var a = new Vector2[len];
            for (var i = 0; i < len; i++) a[i] = RVector2();

            return a;
        }

        /// <summary>
        ///     写Vector2数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(Vector2[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读Vector3
        /// </summary>
        /// <returns></returns>
        public Vector3 RVector3()
        {
            return new Vector3
            {
                x = RFloat(),
                y = RFloat(),
                z = RFloat()
            };
        }

        /// <summary>
        ///     写Vector3
        /// </summary>
        /// <param name="arr"></param>
        public void Write(Vector3 vec)
        {
            Write(vec.x);
            Write(vec.y);
            Write(vec.z);
        }

        /// <summary>
        ///     读Vector3数组
        /// </summary>
        /// <returns></returns>
        public Vector3[] RVector3s()
        {
            var len = (int)RUInt16();
            var a = new Vector3[len];
            for (var i = 0; i < len; i++) a[i] = RVector3();

            return a;
        }

        /// <summary>
        ///     写Vector3数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(Vector3[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读Vector4
        /// </summary>
        /// <param name="arr"></param>
        public Vector4 RVector4()
        {
            return new Vector4
            {
                x = RFloat(),
                y = RFloat(),
                z = RFloat(),
                w = RFloat()
            };
        }

        /// <summary>
        ///     写Vector4
        /// </summary>
        /// <param name="arr"></param>
        public void Write(Vector4 vec)
        {
            Write(vec.x);
            Write(vec.y);
            Write(vec.y);
            Write(vec.w);
        }

        /// <summary>
        ///     读Vector4数组
        /// </summary>
        /// <param name="arr"></param>
        public Vector4[] RVector4s()
        {
            var len = (int)RUInt16();
            var a = new Vector4[len];
            for (var i = 0; i < len; i++) a[i] = RVector4();

            return a;
        }

        /// <summary>
        ///     写Vector4数组
        /// </summary>
        /// <param name="arr"></param>
        public void Write(Vector4[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读Quaternion
        /// </summary>
        /// <returns></returns>
        public Quaternion RQuaternion()
        {
            return new Quaternion
            {
                x = RFloat(),
                y = RFloat(),
                z = RFloat(),
                w = RFloat()
            };
        }

        /// <summary>
        ///     写Quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        public void Write(Quaternion quaternion)
        {
            Write(quaternion.x);
            Write(quaternion.y);
            Write(quaternion.y);
            Write(quaternion.w);
        }

        /// <summary>
        ///     读Rect
        /// </summary>
        /// <returns></returns>
        public Rect RRect()
        {
            return new Rect
            (
                RFloat(),
                RFloat(),
                RFloat(),
                RFloat()
            );
        }

        /// <summary>
        ///     写Rect
        /// </summary>
        /// <param name="rect"></param>
        public void Write(Rect rect)
        {
            Write(rect.x);
            Write(rect.y);
            Write(rect.width);
            Write(rect.height);
        }

        /// <summary>
        ///     读Bounds
        /// </summary>
        /// <returns></returns>
        public Bounds RBounds()
        {
            var center = RVector3();
            var size = RVector3();
            return new Bounds(center, size);
        }

        /// <summary>
        ///     写Bounds
        /// </summary>
        /// <param name="bs"></param>
        public void Write(Bounds bs)
        {
            Write(bs.center);
            Write(bs.size);
        }

        /// <summary>
        ///     读Color
        /// </summary>
        /// <returns></returns>
        public Color RColor()
        {
            return new Color
            {
                a = RFloat(),
                r = RFloat(),
                g = RFloat(),
                b = RFloat()
            };
        }

        /// <summary>
        ///     写Color
        /// </summary>
        /// <param name="c"></param>
        public void Write(Color c)
        {
            Write(c.a);
            Write(c.r);
            Write(c.g);
            Write(c.b);
        }

        /// <summary>
        ///     读Color数组
        /// </summary>
        /// <returns></returns>
        public Color[] RColors()
        {
            var len = (int)RUInt16();
            var a = new Color[len];
            for (var i = 0; i < len; i++) a[i] = RColor();

            return a;
        }

        /// <summary>
        ///     写Color数组
        /// </summary>
        /// <param name="c"></param>
        public void Write(Color[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读Color32
        /// </summary>
        /// <returns></returns>
        public Color32 RColor32()
        {
            return new Color32
            {
                a = RByte(),
                r = RByte(),
                g = RByte(),
                b = RByte()
            };
        }

        /// <summary>
        ///     写Color32
        /// </summary>
        /// <param name="c"></param>
        public void Write(Color32 c)
        {
            Write(c.a);
            Write(c.r);
            Write(c.g);
            Write(c.b);
        }

        /// <summary>
        ///     读Color32数组
        /// </summary>
        /// <returns></returns>
        public Color32[] RColor32s()
        {
            var len = (int)RUInt16();
            var a = new Color32[len];
            for (var i = 0; i < len; i++) a[i] = RColor32();

            return a;
        }

        /// <summary>
        ///     写Color32数组
        /// </summary>
        /// <param name="c"></param>
        public void Write(Color32[] arr)
        {
            Write((ushort)arr.Length);
            var len = arr.Length;
            for (var i = 0; i < len; i++) Write(arr[i]);
        }

        /// <summary>
        ///     读Keyframe
        /// </summary>
        /// <returns></returns>
        public Keyframe RKeyframe()
        {
            var kf = new Keyframe
            {
                inTangent = RFloat(),
                outTangent = RFloat(),
                time = RFloat(),
                value = RFloat()
            };


            return kf;
        }

        /// <summary>
        ///     写Keyframe
        /// </summary>
        /// <param name="kf"></param>
        public void Write(Keyframe kf)
        {
            Write(kf.inTangent);
            Write(kf.outTangent);
            Write(kf.time);
            Write(kf.value);
        }

        /// <summary>
        ///     读AnimationCurve
        /// </summary>
        /// <returns></returns>
        public AnimationCurve RAnimationCurve()
        {
            var len = RByte();
            var kfs = new Keyframe[len];

            for (var i = 0; i < len; i++) kfs[i] = RKeyframe();

            return new AnimationCurve(kfs);
        }

        /// <summary>
        ///     写AnimationCurve
        /// </summary>
        /// <param name="ac"></param>
        public void Write(AnimationCurve ac)
        {
            var len = ac.keys.Length;
            Write((byte)len);
            for (var i = 0; i < len; i++) Write(ac.keys[i]);
        }

        public Matrix4x4 RMatrix4x4()
        {
            var matrix = new Matrix4x4();
            for (var i = 0; i < 16; i++) matrix[i] = RFloat();

            return matrix;
        }

        public void Write(Matrix4x4 matrix)
        {
            for (var i = 0; i < 16; i++) Write(matrix[i]);
        }
    }
}