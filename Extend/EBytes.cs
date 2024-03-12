using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Cherry.Extend
{
    public static class EBytes
    {
        public static string ToHex(this byte[] bytes)
        {
            if (bytes == null) return "";

            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static bool[] ToBits(this byte[] bytes)
        {
            var bits = new bool[bytes.Length * 8];
            for (var i = 0; i < bytes.Length; i++)
            {
                var bools = bytes[i].ToBits();
                Array.Copy(bools, 0, bits, i * 8, 8);
            }

            return bits;
        }

        public static string ToUTF8(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToUTF8(this byte[] bytes, int offset, int length)
        {
            return Encoding.UTF8.GetString(bytes, offset, length);
        }

        public static int ReadIntLE(this byte[] bytes, int length, int offset = 0)
        {
            var count = 0;
            var val = 0;
            for (var i = length - 1; i > -1; i--) val |= bytes[i + offset] << (8 * count++);

            return val;
        }

        public static void WriteIntLE(this byte[] bytes, int val, int length)
        {
            var count = 0;
            for (var i = length - 1; i > -1; i--) bytes[i] = (byte)(val >> (8 * count++));
        }

        public static void CopyToTextureBlock(this byte[] src, int sx, int sy, int width, int height,
            int srcTextureWidth, byte[] dst, int dx, int dy, int dstTextureWidth, int block, int blockBytes)
        {
            sx /= block;
            sy /= block;
            srcTextureWidth /= block;
            dx /= block;
            dy /= block;
            dstTextureWidth /= block;
            width /= block;
            height /= block;

            for (var i = 0; i < height; i++)
            {
                var si = (sx + (sy + i) * srcTextureWidth) * blockBytes;
                var di = (dx + (dy + i) * dstTextureWidth) * blockBytes;
                Buffer.BlockCopy(src, si, dst, di, width * blockBytes);
            }
        }

        public static byte[] GZipCompress(this byte[] bytes)
        {
            using (var originStream = new MemoryStream(bytes))
            {
                using (var compressStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                    {
                        originStream.CopyTo(zipStream);
                    }

                    return compressStream.ToArray();
                }
            }
        }

        public static byte[] GZipDecompress(this byte[] bytes)
        {
            using (var compressStream = new MemoryStream(bytes))
            {
                using (var decompressStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(decompressStream);
                    }

                    return decompressStream.ToArray();
                }
            }
        }
    }
}