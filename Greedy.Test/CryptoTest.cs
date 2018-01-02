using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
//using System.Web.Security;

namespace Greedy.Test
{
    [TestClass]
    public class CryptoTest
    {
        //[TestMethod]
        //public void TestMD5()
        //{
        //    var source = "mytest123456";
        //    var md5 = new MD5CryptoServiceProvider();
        //    byte[] hashed = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
        //    var displayString = new StringBuilder();
        //    foreach (byte iByte in hashed)
        //    {
        //        displayString.AppendFormat("{0:x2}", iByte);
        //    }

        //    var sign = displayString.ToString();


        //    var sign2 = FormsAuthentication.HashPasswordForStoringInConfigFile(source, "md5");

        //    Assert.AreEqual(source.ToMd5().ToLower(), sign);
        //    Assert.AreEqual(sign, sign2.ToLower());
        //}
    }
}