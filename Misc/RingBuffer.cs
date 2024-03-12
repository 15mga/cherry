using System;
using System.Collections.Generic;
using System.Linq;

namespace Cherry.Misc
{
    /// <summary>
    ///     环形缓冲
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer<T>
    {
        private readonly T[] buffer;

        private bool flipped;

        private int readPos;

        private int writePos;

        public RingBuffer() : this(2048)
        {
        }

        public RingBuffer(int cap)
        {
            this.cap = cap;
            buffer = new T[cap];
        }

        public int cap { get; }

        public int remains => flipped ? readPos - writePos : cap - writePos + readPos;

        public int available => flipped ? writePos + cap - readPos : writePos - readPos;

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            flipped = false;
            readPos = 0;
            writePos = 0;
        }

        private void GrowWrite(int val)
        {
            writePos += val;
            if (writePos < cap) return;
            flipped = true;
            writePos -= cap;
        }

        private void GrowRead(int val)
        {
            readPos += val;
            if (readPos < cap) return;
            flipped = false;
            readPos -= cap;
        }

        /// <summary>
        ///     写入元素,如果缓冲不够返回false
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Write(T item)
        {
            if (remains == 0) return false;
            buffer[writePos] = item;
            GrowWrite(1);
            return true;
        }

        /// <summary>
        ///     批量写入元素,如果缓冲不够返回false
        /// </summary>
        /// <param name="items"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool Write(T[] items, int offset = 0, int length = 0)
        {
            length = length > 0 ? length : items.Length;
            if (remains < length) return false;
            Array.Copy(items, offset, buffer, writePos, length);
            GrowWrite(length);
            return true;
        }

        /// <summary>
        ///     批量写入元素,如果缓冲不够返回false
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool Write(IEnumerable<T> items)
        {
            var length = items.Count();
            if (remains < length) return false;
            foreach (var item in items) buffer[writePos] = item;
            GrowWrite(length);
            return true;
        }

        /// <summary>
        ///     写入源RingBuffer
        ///     available或者source.available小于length都会返回失败
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length">等于0时,默认等于source.available</param>
        /// <returns></returns>
        public bool Write(RingBuffer<T> source, int length = 0)
        {
            if (length > 0)
            {
                if (length > source.available) return false;
            }
            else
            {
                length = source.available;
            }

            if (remains < length) return false;

            var dCap = cap - writePos;
            var sCap = source.cap - source.readPos;
            if (dCap < length)
            {
                if (sCap < length)
                {
                    if (dCap < sCap)
                    {
                        Array.Copy(source.buffer, source.readPos, buffer, writePos, dCap);
                        var len = sCap - dCap;
                        Array.Copy(source.buffer, source.readPos + dCap, buffer, 0, len);
                        Array.Copy(source.buffer, 0, buffer, len, length - len - dCap);
                    }
                    else
                    {
                        Array.Copy(source.buffer, source.readPos, buffer, writePos, sCap);
                        var len = dCap - sCap;
                        Array.Copy(source.buffer, 0, buffer, writePos + sCap, len);
                        Array.Copy(source.buffer, len, buffer, 0, length - len - sCap);
                    }
                }
                else
                {
                    Array.Copy(source.buffer, source.readPos, buffer, writePos, dCap);
                    Array.Copy(source.buffer, source.readPos + dCap, buffer, 0, length - dCap);
                }
            }
            else
            {
                if (sCap < length)
                {
                    Array.Copy(source.buffer, source.readPos, buffer, writePos, sCap);
                    Array.Copy(source.buffer, 0, buffer, writePos + sCap, length - sCap);
                }
                else
                {
                    Array.Copy(source.buffer, source.readPos, buffer, writePos, length);
                }
            }

            source.GrowRead(length);
            GrowWrite(length);

            return true;
        }

        /// <summary>
        ///     读取元素
        /// </summary>
        /// <param name="items"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool Read(T[] items, int offset = 0, int length = 0)
        {
            length = length > 0 ? length : items.Length;
            if (offset + length > items.Length) throw new ArgumentOutOfRangeException(nameof(length));
            if (available < length) return false;

            var cp = cap - readPos;
            if (cp < length)
            {
                Array.Copy(buffer, readPos, items, offset, cp);
                Array.Copy(buffer, 0, items, offset + cap, length - cp);
            }
            else
            {
                Array.Copy(buffer, readPos, items, offset, length);
            }

            GrowRead(length);
            return true;
        }

        /// <summary>
        ///     读取元素
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public T[] Read(int length)
        {
            if (length == 0) throw new ArgumentException(nameof(length));
            if (available < length) throw new ArgumentOutOfRangeException();

            var items = new T[length];
            var cp = cap - readPos;
            if (cp < length)
            {
                Array.Copy(buffer, readPos, items, 0, cp);
                Array.Copy(buffer, 0, items, cp, length - cp);
            }
            else
            {
                Array.Copy(buffer, readPos, items, 0, length);
            }

            GrowRead(length);
            return items;
        }

        /// <summary>
        ///     读取所有元素
        /// </summary>
        /// <returns></returns>
        public T[] ReadAll()
        {
            return Read(available);
        }
    }
}