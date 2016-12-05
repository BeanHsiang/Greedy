using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Util
{
    static class StreamExtension
    {
        public static void WriteString(this Stream stream, string content, bool isNullTerminated = false)
        {
            if (string.IsNullOrEmpty(content)) return;
            var byts = Encoding.Default.GetBytes(content);
            stream.Write(byts, 0, byts.Length);
            if (isNullTerminated) { stream.WriteByte(0x00); }
        }

        public static void WriteInt(this Stream stream, int num)
        {
            var byts = BitConverter.GetBytes(num);
            stream.Write(byts, 0, byts.Length);
        }

        public static void WriteShort(this Stream stream, short num)
        {
            var byts = BitConverter.GetBytes(num);
            stream.Write(byts, 0, byts.Length);
        }

        public static void WriteLong(this Stream stream, long num)
        {
            var byts = BitConverter.GetBytes(num);
            stream.Write(byts, 0, byts.Length);
        }

        public static void WriteFixedLengthInt(this Stream stream, long num, int length)
        {
            var byts = BitConverter.GetBytes(num);
            stream.Write(byts, 0, length);
        }

        public static void WriteLengthEncodedInt(this Stream stream, long num)
        {
            var byts = DataType.GetLengthEncodedInt(num);
            stream.Write(byts, 0, byts.Length);
        }

        public static void WriteLengthEncodedString(this Stream stream, string content)
        {
            var byts = DataType.GetLengthEncodedString(content);
            stream.Write(byts, 0, byts.Length);
        }

        public static int ReadInt(this Stream stream)
        {
            var byts = new byte[4];
            stream.Read(byts, 0, byts.Length);
            return BitConverter.ToInt32(byts, 0);
        }

        public static short ReadShort(this Stream stream)
        {
            var byts = new byte[2];
            stream.Read(byts, 0, byts.Length);
            return BitConverter.ToInt16(byts, 0);
        }

        public static long ReadLengthEncodedInt(this Stream stream)
        {
            var byt = stream.ReadByte();
            byte[] byts = null;
            if (byt <= 0xfb)
            {
                return byt;
            }
            else if (byt == 0xfc)
            {
                byts = new byte[2];
            }
            else if (byt == 0xfd)
            {
                byts = new byte[3];
            }
            else if (byt == 0xfe)
            {
                byts = new byte[8];
            }
            stream.Read(byts, 0, byts.Length);
            return BitConverter.ToInt32(byts, 0);
        }

        public static string ReadFixedString(this Stream stream, long length)
        {
            var byts = new byte[length];
            stream.Read(byts, 0, byts.Length);
            return Encoding.Default.GetString(byts);
        }

        public static byte[] ReadFixedBytes(this Stream stream, long length)
        {
            var byts = new byte[length];
            stream.Read(byts, 0, byts.Length);
            return byts;
        }

        public static string ReadNulTerminatedString(this Stream stream)
        {
            return Encoding.Default.GetString(stream.ReadNulTerminatedBytes());
        }

        public static byte[] ReadNulTerminatedBytes(this Stream stream)
        {
            var byts = new List<byte>();
            int ret;
            while ((ret = stream.ReadByte()) > 0x00)
            {
                byts.Add((byte)ret);
            }
            return byts.ToArray();
        }

        public static string ReadLengthEncodedString(this Stream stream)
        {
            var length = stream.ReadLengthEncodedInt();
            return length == 0xfb ? null : stream.ReadFixedString(length);
        }

        public static string ReadRestOfPacketString(this Stream stream)
        {
            var byts = new List<byte>();
            int ret;
            while ((ret = stream.ReadByte()) != -1)
            {
                byts.Add((byte)ret);
            }
            return Encoding.Default.GetString(byts.ToArray());
        }
    }
}