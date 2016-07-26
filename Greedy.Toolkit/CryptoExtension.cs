using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit
{
    public static class CryptoExtension
    {
        /// <summary> 
        /// MD5 16位加密 
        /// </summary> 
        /// <param name="BitMode">加密位，默认16bit:false 32bit:true</param>
        /// <returns></returns> 
        public static string ToMd5(this string source, bool BitMode = true)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                byte[] byt = md5.ComputeHash(Encoding.Default.GetBytes(source));
                string md5String = string.Empty;
                if (BitMode)
                {
                    md5String = BitConverter.ToString(byt);
                }
                else
                {
                    md5String = BitConverter.ToString(byt, 4, 8);
                }
                md5String = md5String.Replace("-", "");
                return md5String.ToLower();
            }
        }

        public static string ToMd5(this string source, out string salt, bool BitMode = true)
        {
            using (Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(source, 32, 1000))
            {
                salt = Convert.ToBase64String(k1.GetBytes(32));
                return string.Format("{0}{1}", source, salt).ToMd5(BitMode);
            }
        }
    }
}