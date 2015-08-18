using Greedy.Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Greedy.TestConsole
{
    class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var conStr = "server=LocalHost;User Id=root;Pwd=greedyint;database=test";
            var cnn = new MySqlConnection(conStr);
            var person = new Person();
            //cnn.Insert<Person>(person);
            //cnn.Insert<Person>(new { Name = "asdsa", Age = 13, Address = "addoio;l" });
            var dic = new Dictionary<string, object>();
            var dic2 = new Dictionary<string, object>();
            dic.Add("Name", "张三");
            dic2.Add("Name", "李四");
            //Console.WriteLine(dic.GetType());
            //Console.WriteLine(dic2.GetHashCode());
            cnn.Insert<Person>(dic);
            cnn.Insert<Person>(dic2);
            var obj = new { Name = "ddd", Age = 34 };
            var id = cnn.InsertWithIdentity<Person>(obj);
            Console.WriteLine(id);
            cnn.Update<Person>(new { Id = id, Address = "就是这里" });
            //var obj2 = new { NAme = "ddd", AgE = 34 };
            //var obj3 = new { Age = 34, Name = "ddd" };
            //Console.WriteLine(obj.GetType().Name);
            //Console.WriteLine(obj2.GetType().Name);
            //Console.WriteLine(obj3.GetType().Name);
        }

    }
}
