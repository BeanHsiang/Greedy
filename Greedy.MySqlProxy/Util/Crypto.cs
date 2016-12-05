using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Util
{
    static class Crypto
    {
        public static byte[] Sha1(byte[] data)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return sha1.ComputeHash(data);
            }
        }

        public static byte[] Sha1(string data)
        {
            var byts = Encoding.Default.GetBytes(data);
            return Sha1(byts);
        }
    }
}
