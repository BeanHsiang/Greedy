using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
//using System.Web.Security;
using System.Diagnostics;

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

        [TestMethod]
        public void TestNull()
        {
            var p = GetPerson();
            var code = Type.GetTypeHandle(p);
            Debug.WriteLine(code);
        }

        [TestMethod]
        public void TestTimeSpan()
        {
            string time1 = "2010-5-26";
            string time2 = "2011-6-26";
            DateTime t1 = Convert.ToDateTime(time1);
            DateTime t2 = Convert.ToDateTime(time2);
 
            TimeSpan interval =  t2 - t1;
            Console.WriteLine("Value of TimeSpan: {0}", interval);

            Debug.WriteLine("{0:N5} days, as follows:", interval.TotalDays);
            Debug.WriteLine("   Days:         {0,3}", interval.Days);
            Debug.WriteLine("   Hours:        {0,3}", interval.Hours);
            Debug.WriteLine("   Minutes:      {0,3}", interval.Minutes);
            Debug.WriteLine("   Seconds:      {0,3}", interval.Seconds);
            Debug.WriteLine("   Milliseconds: {0,3}", interval.Milliseconds);
        }


        Person GetPerson()
        {
            return null;
        }
    }
}