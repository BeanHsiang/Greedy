using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Util
{
    static class DataType
    {
        public static byte[] GetLengthEncodedInt(long num)
        {
            var ret = new List<byte>();
            var byts = BitConverter.GetBytes(num);
            if (num < 0xfb)
            {
                ret.Add(byts[0]);
            }
            else if (num < 0x10000)
            {
                ret.Add(0xfc);
                ret.AddRange(byts.Take(2));
            }
            else if (num < 0x1000000)
            {
                ret.Add(0xfd);
                ret.AddRange(byts.Take(3));
            }
            else
            {
                ret.Add(0xfe);
                ret.AddRange(byts.Take(8));
            }

            return ret.ToArray();
        }

        public static byte[] GetLengthEncodedString(string content)
        {
            var ret = new List<byte>();

            var byts = Encoding.Default.GetBytes(content);

            ret.AddRange(GetLengthEncodedInt(byts.Length));
            ret.AddRange(byts);

            return ret.ToArray();
        }
    }
}
