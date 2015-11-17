using Greedy.Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using System.Linq;
using System.Linq.Expressions;

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

            //var person = new Person() { Name = "name1" };
            //var person2 = new Person() { Name = "name1" };
            //cnn.Insert<Person>(new[] { person, person2 });
            cnn.Update<Person>(p => p.Id > 10, new Dictionary<Expression<Func<Person, object>>, object> {
                {p=> p.Address, "mytest"},
                {p=> p.Age, 14}
            });
            //cnn.Insert<Person>(new { Name = "asdsa", Age = 13, Address = "addoio;l" });
            //var dic = new Dictionardsy<string, object>();
            //var dic2 = new Dictionary<string, object>();
            //dic.Add("Name", "张三");
            //dic2.Add("Name", "李四");
            //Console.WriteLine(dic.GetType());
            //Console.WriteLine(dic2.GetHashCode());
            //var tran = new CommittableTransaction();
            //cnn.Open();
            //cnn.Insert<Person>(dic);
            //cnn.Insert<Person>(dic2);
            //cnn.EnlistTransaction(tran);
            //var obj = new { Name = "ddd1", Age = DateTime.Now.Minute };
            //var id = cnn.InsertWithIdentity<Person>(obj);
            //Console.WriteLine(id);
            //cnn.Update<Person>(new { Id = id, Address = "就是那里" });
            //cnn.Delete<Person>(p => p.Address == "ad" || (p.Age >= 19 && p.Age < 30));
            cnn.Close();
            //tran.Commit();
            //var obj2 = new { NAme = "ddd", AgE = 34 };
            //var obj3 = new { Age = 34, Name = "ddd" };
            //Console.WriteLine(obj2.GetType().Name);s
            //Console.WriteLine(obj.GetType().Name);
            //Console.WriteLine(obj3.GetType().Name);


        }

    }
}