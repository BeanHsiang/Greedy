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

    class Male : Person
    {

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
            //cnn.Update<Person>(p => p.Id > 10, new Dictionary<Expression<Func<Person, object>>, object> {
            //    {p=> p.Address, "mytest"},
            //    {p=> p.Age, 14}
            //});
            var per = new Person { Id = 5 };
            var id = 7;
            var ids = new long[] { 9, 14, 15 };
            var names = new List<string>(new[] { "ad2", "ad2ddfff" });
            names.Add("ad2");
            names.Add("ad2ddfff");
            //Console.WriteLine(cnn.Execute("insert into Person(Name) values(@Name)", new[] { new   { Name = "M1" }, new   { Name = "p1" } }));
            //Console.WriteLine(cnn.Get<Person>(p => ids.Contains((int)p.Id)).Count());
            var targetEntity = new Person { Id = 7, Address = "Add3r", Name = "ddd1", Age = 2 };
            var dic = new Dictionary<Expression<Func<Person, object>>, object>();

            dic.Add(p => p.Name, targetEntity.Name);
            dic.Add(p => p.Address, targetEntity.Address);
            dic.Add(p => p.Age, targetEntity.Age);

            var str = "6";
            //Console.WriteLine(cnn.Update<Person>(p => p.Id < targetEntity.Id && p.Age >= targetEntity.Age && targetEntity.Name.Contains(p.Name), dic));
            //cnn.Insert<Person>(new Person[] { new Person { Name = "M1" }, new Person { Name = "p1" } });
            Console.WriteLine(cnn.Get<Person>(p => p.Id == long.Parse(str)).Count());
            //Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && "hz".IndexOf(p.Address) > -1).Count());
            //Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && ids[0] > p.Id).Count());
            //Console.WriteLine(cnn.Get<Person>(p => per.Id == p.Id).Count());
            //Console.WriteLine(cnn.Get<Person>(p => names.Contains(p.Name)).Count());
            //Console.WriteLine(cnn.Get<Person>(p => p.Name.Contains("2d")).Count());
            //Console.WriteLine(cnn.Get<Person>(p => "ad2".Equals(p.Name)).Count());
            //Console.WriteLine(cnn.Get<Person>(p => new[] { "ad2", "ad2ddfff" }.Contains(p.Name)).Count());
            //Console.WriteLine(cnn.Get<Person>(p => new List<string>(new[] { "ad2", "ad2ddfff" }).Contains(p.Name)).Count());
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
            //var age = 30;
            //cnn.Delete<Person>(p => p.Address == "ad" || (p.Age >= 19 && p.Age < age));
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