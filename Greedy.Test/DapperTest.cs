using Greedy.Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Greedy.Test
{
    class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
    }

    [TestClass]
    public class DapperTest
    {
        static IDbConnection con;
        Random rand = new Random();


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var conStr = "server=LocalHost;User Id=root;Pwd=greedyint;database=test";
            con = new MySqlConnection(conStr);
        }

        [ClassCleanup]
        public static void Clean()
        {
            con.Dispose();
        }

        [TestMethod]
        public void TestInsertStrongClassInstance()
        {
            var person = new Person() { Name = "TestInsertStrongClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertStrongClassInstanceAddress" };

            var count = con.Insert<Person>(person);
            Assert.AreEqual(1, count, "插入强类型实例失败");
        }

        [TestMethod]
        public void TestInsertAnonymousClassInstance()
        {
            var person = new { Name = "TestInsertAnonymousClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertAnonymousClassInstanceAddress" };

            var count = con.Insert<Person>(person);
            Assert.AreEqual(1, count, "插入匿名类型实例失败");
        }

        [TestMethod]
        public void TestInsertBatchStrongClassInstance()
        {
            var person = new Person() { Name = "TestInsertBatchStrongClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertBatchStrongClassInstanceAddress" };

            var person2 = new Person() { Name = "TestInsertBatchStrongClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertBatchStrongClassInstanceAddress" };

            var count = con.Insert<Person>(new Person[] { person, person2 });
            Assert.AreEqual(2, count, "批量插入强类型实例失败");
        }

        [TestMethod]
        public void TestInsertBatchAnonymousClassInstance()
        {
            /* 匿名类型的实例，字段名称和个数必须一致 */
            var person = new { Name = "TestInsertBatchAnonymousClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertBatchAnonymousClassInstanceAddress" };

            var person2 = new { Name = "TestInsertBatchAnonymousClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertBatchAnonymousClassInstanceAddress" };

            var count = con.Insert<Person>(new[] { person, person2 });
            Assert.AreEqual(2, count, "批量插入匿名类型实例失败");
        }

        [TestMethod]
        public void TestInsertDictionaryClassInstance()
        {
            var dic = new Dictionary<string, object>();

            dic.Add("Name", "TestInsertDictionaryClassInstanceName");
            dic.Add("Age", rand.Next(1, 100));
            dic.Add("Address", "TestInsertDictionaryClassInstanceAddress");

            var count = con.Insert<Person>(dic);
            Assert.AreEqual(1, count, "插入字典形式类型实例失败");
        }

        [TestMethod]
        public void TestUpdateStrongClassInstance()
        {
            var person = con.Query<Person>("select * from Person order by id desc limit 1;").First();

            person.Name = "TestUpdateStrongClassInstanceName";

            person.Address = "TestUpdateStrongClassInstanceAddress";

            var count = con.Update<Person>(person);
            Assert.AreEqual(1, count, "更新强类型实例失败");
        }

        [TestMethod]
        public void TestUpdateAnonymousClassInstance()
        {
            var person = con.Query<Person>("select * from Person order by id desc limit 1;").First();
            var updatePerson = new { Id = person.Id, Age = person.Age + 1, Address = "TestUpdateAnonymousClassInstanceAddress" };

            var count = con.Update<Person>(updatePerson);
            Assert.AreEqual(1, count, "更新匿名类型实例失败");
        }

        [TestMethod]
        public void TestUpdateExpression()
        {
            var person = con.Query<Person>("select * from Person order by id desc limit 1;").First();
            var count = con.Update<Person>(p => p.Id == person.Id, t => t.Address, "TestUpdateExpressionAddress");
            Assert.AreEqual(1, count, "更新带表达式条件的实例失败");

            var count2 = con.Set<Person>()
                .Where(p => p.Id == person.Id)
                .Set(t => t.Address, "TestUpdateExpressionAddress")
                .Set(t => t.Age, person.Age + 1)
                .Update();

            Assert.AreEqual(1, count2, "更新带表达式条件的实例失败");
        }

        [TestMethod]
        public void TestInsertWithIdentityStrongClassInstance()
        {
            var lastPerson = con.Query<Person>("select * from Person order by id desc limit 1;").First();

            var person = new Person() { Name = "TestInsertWithIdentityStrongClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertWithIdentityStrongClassInstanceAddress" };

            var id = con.InsertWithIdentity<Person>(person);
            Assert.AreEqual(true, lastPerson.Id < id, "插入强类型实例获取Id失败");
        }

        [TestMethod]
        public void TestInsertWithIdentityAnonymousClassInstance()
        {
            var lastPerson = con.Query<Person>("select * from Person order by id desc limit 1;").First();

            var person = new { Name = "TestInsertWithIdentityAnonymousClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertWithIdentityAnonymousClassInstanceAddress" };

            var id = con.InsertWithIdentity<Person>(person);
            Assert.AreEqual(true, lastPerson.Id < id, "插入匿名类型实例获取Id失败");
        }

        [TestMethod]
        public void TestInsertWithIdentityDictionaryClassInstance()
        {
            var lastPerson = con.Query<Person>("select * from Person order by id desc limit 1;").First();

            var dic = new Dictionary<string, object>();

            dic.Add("Name", "TestInsertWithIdentityDictionaryClassInstanceName");
            dic.Add("Age", rand.Next(1, 100));
            dic.Add("Address", "TestInsertWithIdentityDictionaryClassInstanceAddress");

            var id = con.InsertWithIdentity<Person>(dic);
            Assert.AreEqual(true, lastPerson.Id < id, "插入字典形式类型实例获取Id失败");
        }

        //[TestMethod]
        public void TestDelete()
        {
            var sql = "select count(id) from person";
            var rowsCount = con.ExecuteScalar<int>(sql);
            var age = 30;
            var count = con.Delete<Person>(p => p.Address == "ad" || (p.Age >= 19 && p.Age < age));
            var rowsCount2 = con.ExecuteScalar<int>(sql);

            Assert.AreEqual(rowsCount - rowsCount2, count, "删除失败");
        }

        [TestMethod]
        public void TestGetWithLocalVariable()
        {
            var age = 30;
            var sql = "select count(id) from person where age > " + age;
            var rowsCount = con.ExecuteScalar<int>(sql);

            var count = con.Get<Person>(p => p.Age >= age && p.Age < 100).Count();
            Assert.AreEqual(rowsCount, count, "获取带本地变量的查询失败");
        }

        [TestMethod]
        public void TestGetWithLocalClassInstance()
        {
            var age = 30;
            var man = new { Age = 30 };
            var person = new Person { Age = 30 };
            var sql = "select count(id) from person where age > " + age;
            var rowsCount = con.ExecuteScalar<int>(sql);

            var count1 = con.Get<Person>(p => p.Age >= man.Age && p.Age - 1 < 100).Count();
            Assert.AreEqual(rowsCount, count1, "获取带本地类实例变量的查询失败");
            var count2 = con.Get<Person>(p => p.Age >= person.Age && p.Age - 1 < 100).Count();
            Assert.AreEqual(rowsCount, count2, "获取带本地匿名类实例变量的查询失败");
        }

        [TestMethod]
        public void TestGetWithMethodContains()
        {
            var count = con.Get<Person>(p => p.Name.Contains("test")).Count();
            Assert.AreNotEqual(0, count, "获取带Contains方法的查询失败");
        }
    }
}  //Console.WriteLine(cnn.Update<Person>(p => p.Id < targetEntity.Id && p.Age >= targetEntity.Age && targetEntity.Name.Contains(p.Name), dic));
//cnn.Insert<Person>(new Person[] { new Person { Name = "M1" }, new Person { Name = "p1" } });
//var q = from p in cnn.Query<Person>()
//        from m in cnn.Query<Article>()
//        where p.Id == m.PersonId
//        select p;
//var q1 = cnn.Query<Person>().Where(p => p.Address == "asd").Where(p => p.Age > 32);
//Console.WriteLine(q1.ToArray().Count());
//Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && "hz".IndexOf(p.Address) > -1).Count());
//Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && ids[0] > p.Id).Count());
//Console.WriteLine(cnn.Get<Person>(p => per.Id == p.Id).Count());
//Console.WriteLine(cnn.Get<Person>(p => names.Contains(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => p.Name.Contains("2d")).Count());
//Console.WriteLine(cnn.Get<Person>(p => "ad2".Equals(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => new[] { "ad2", "ad2ddfff" }.Contains(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => new List<string>(new[] { "ad2", "ad2ddfff" }).Contains(p.Name)).Count());