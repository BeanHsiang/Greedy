using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Util
{
    static class ByteExtension
    {
        public static byte[] Xor(this byte[] first, byte[] second)
        {
            byte[] rBytes = new byte[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                rBytes[i] = (byte)(first[i] ^ second[i]);
            }

            return rBytes;
        }

        public static bool Compare(this byte[] first, byte[] second)
        {
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
