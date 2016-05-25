using Greedy.Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Greedy.Test
{
    class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
    }

    class Article
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public long AuthorId { get; set; }
        public long CatetoryId { get; set; }
    }

    class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    [TestClass]
    public class DapperTest
    {
        static IDbConnection con;
        Random rand = new Random();
        const string conStr = "server=LocalHost;User Id=root;Pwd=greedyint;database=test";


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
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
            var sql = "select count(id) from person where age >= " + age + " and age < 100";
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
            var sql = "select count(id) from person where age >= " + age + " and age < 100";
            var rowsCount = con.ExecuteScalar<int>(sql);

            var count1 = con.Get<Person>(p => p.Age >= man.Age && p.Age < 100).Count();
            Assert.AreEqual(rowsCount, count1, "获取带本地类实例变量的查询失败");
            var count2 = con.Get<Person>(p => p.Age >= person.Age && p.Age < 100).Count();
            Assert.AreEqual(rowsCount, count2, "获取带本地匿名类实例变量的查询失败");
        }

        [TestMethod]
        public void TestGetWithMethodContains()
        {
            var count = con.Get<Person>(p => p.Name.Contains("test")).Count();
            Assert.AreNotEqual(0, count, "获取带Contains方法的查询失败");

            var name = "TestInsertStrongClassInstanceName";
            var count2 = con.Get<Person>(p => name.Contains(p.Name)).Count();
            Assert.AreNotEqual(0, count2, "获取带Contains方法的查询失败");

            var count3 = con.Get<Person>(p => "TestInsertStrongClassInstanceName".Contains(p.Name)).Count();
            Assert.AreNotEqual(0, count3, "获取带Contains方法的查询失败");
        }

        [TestMethod]
        public void TestSingleLinq()
        {
            var arr = con.Predicate<Person>().ToList();
            Assert.AreNotEqual(0, arr.Count, "Linq单表全表查询失败");

            var arr2 = con.Predicate<Person>().Where(p => p.Age > 30).ToList();
            Assert.AreNotEqual(0, arr2.Count, "Linq单表带Where查询失败");

            var arr3 = con.Predicate<Person>().Where(p => p.Age > 30 && p.Age < 50).Select(p => new { p.Id, p.Name }).ToList();
            Assert.AreNotEqual(0, arr3.Count, "Linq单表带Where匿名查询失败");

            var arr4 = con.Predicate<Person>().Where(p => p.Age > 30 && p.Age < 90).Select(p => new { p.Id, p.Name }).Skip(2).Take(10).ToList();
            Assert.AreNotEqual(0, arr4.Count, "Linq单表带Where带分页匿名查询失败");

            var arr5 = con.Predicate<Person>().Where(p => p.Age > 30 && p.Age < 90).Count(p => p.Address.Contains("address"));
            Assert.AreNotEqual(0, arr5, "Linq单表带Where求行数查询失败");

            var arr6 = con.Predicate<Person>().Where(p => p.Age > 30 && p.Age < 90).Any();
            Assert.IsTrue(arr6, "Linq单表带Where求是否存在查询失败");
        }

        [TestMethod]
        public void TestCrossJoinTablesLinq()
        {
            var query = from p in con.Predicate<Person>()
                        from art in con.Predicate<Article>().Where(a => a.Content.Contains("test"))
                        from cat in con.Predicate<Category>()
                        where p.Id == art.AuthorId && art.CatetoryId == cat.Id
                        select new { art.Id, art.Name, AuthorName = p.Name, CategoryName = cat.Name };
            //var nonEquijoinQuery =
            //    from art in con.Predicate<Article>()
            //    let catIds = from c in con.Predicate<Category>()
            //                 select c.Id
            //    where catIds.Contains(art.CatetoryId) == true
            //    select new { Product = art.Name, CategoryID = art.CatetoryId }; 
            Assert.AreNotEqual(0, query.Count(), "简单连接多表Linq查询失败");
        }

        [TestMethod]
        public void TestInnerJoinTablesLinq()
        {
            var query2 = from art in con.Predicate<Article>()
                         join p in con.Predicate<Person>() on art.AuthorId equals p.Id
                         join cat in con.Predicate<Category>() on art.CatetoryId equals cat.Id
                         select new { art.Id, art.Name, AuthorName = p.Name, CategoryName = cat.Name };
            Assert.AreNotEqual(0, query2.Count(), "InnerJoin多表Linq查询失败");

            var arr = con.Predicate<Person>()
               .Join(con.Predicate<Article>(), p => new { Id = p.Id, p.Name }, t => new { Id = t.AuthorId, t.Name }, (p, t) => new { AuthorName = p.Name, ArticleId = t.Id, CategoryId = t.CatetoryId })
               .Join(con.Predicate<Category>(), p => p.CategoryId, t => t.Id, (p, t) => new { Id = p.ArticleId, AuthorName = p.AuthorName, CategoryName = t.Name })
               .ToList();
            Assert.AreNotEqual(0, arr.Count, "InnerJoin多表Linq查询失败");
        }

        [TestMethod]
        public void TestLeftJoinTablesLinq()
        {
            //var arr = con.Predicate<Person>()
            //    .Join(con.Predicate<Article>(), p => new { Id = p.Id, p.Name }, t => new { Id = t.AuthorId, t.Name }, (p, t) => new { t.AuthorId, AuthorName = p.Name, t.Name })
            //    .ToList();
            //var arr = con.Predicate<Person>()
            //   .Join(con.Predicate<Article>(), p => new { Id = p.Id, p.Name }, t => new { Id = t.AuthorId, t.Name }, (p, t) => new { Person = p, Article = t })
            //   .Join(con.Predicate<Category>(), p => p.Article.CatetoryId, t => t.Id, (p, t) => new { Id = p.Article.Id, AuthorName = p.Person.Name, CategoryName = t.Name })
            //   .ToList();
            var arr = from p in con.Predicate<Person>()
                      join art in con.Predicate<Article>() on p.Id equals art.AuthorId into articles
                      from ar in articles.DefaultIfEmpty()
                      select new { Name = p.Name, ArticleName = ar.Name };

            Assert.AreNotEqual(0, arr.ToArray().Count(), "LeftJoin多表Linq查询失败");

            var arr2 = from p in con.Predicate<Person>()
                       join art in con.Predicate<Article>() on p.Id equals art.AuthorId into articles
                       select new { Name = p.Name, Count = articles.Count() };

            Assert.AreNotEqual(0, arr2.ToArray().Count(), "LeftJoin多表Linq查询失败");
            //var arr = con.Predicate<Person>()
            //   .GroupJoin(con.Predicate<Article>(), p => new { Id = p.Id, p.Name }, t => new { Id = t.AuthorId, t.Name }, (p, t) => new { AuthorName = p.Name, ArticleId = t.Id, CategoryId = t.CatetoryId }) 
            //   .Join(con.Predicate<Category>(), p => p.CategoryId, t => t.Id, (p, t) => new { Id = p.ArticleId, AuthorName = p.AuthorName, CategoryName = t.Name })
            //   .ToList(); 
        }

        [TestMethod]
        public void TestLeftInnerJoinTablesLinq()
        {
            var arr = from art in con.Predicate<Article>()
                      join cat in con.Predicate<Category>() on art.CatetoryId equals cat.Id
                      join p in con.Predicate<Person>() on art.AuthorId equals p.Id into persons
                      from ps in persons.DefaultIfEmpty()
                      select new { Name = ps.Name, ArticleName = art.Name };

            Assert.AreNotEqual(0, arr.ToArray().Count(), "LeftInner多表Linq查询失败");
        }

        [TestMethod]
        public void TestGroupByLinq()
        {
            var arr = from p in con.Predicate<Person>()
                      group p by new { p.Age, p.Address } into ages
                      select new { ages.Key.Age, MaxId = ages.Max(p => p.Id) };

            Assert.AreNotEqual(0, arr.ToArray().Count(), "LeftInner多表Linq查询失败");
        }

        [TestMethod]
        public void TestFirstLinq()
        {
            var person = con.Predicate<Person>().Where(p => p.Age < 20).FirstOrDefault();

            Assert.AreNotEqual(0, person.Id, "First Linq查询失败");
        }

        [TestMethod]
        public void TestInsertStrongClassInstanceWithTask()
        {
            var count = 10;
            var tasks = new Task[count];
            Parallel.For(0, count, i =>
            {
                var person = new Person() { Name = "TestInsertStrongClassInstanceName", Age = rand.Next(1, 100), Address = "TestInsertStrongClassInstanceAddress" };
                tasks[i] = Task.Run(() =>
                {
                    var threadCon = new MySqlConnection(conStr);
                    threadCon.Insert<Person>(person);
                });
            });

            Task.WaitAll(tasks);
        }
    }
}

//Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && "hz".IndexOf(p.Address) > -1).Count());
//Console.WriteLine(cnn.Get<Person>(p => ids.Contains(p.Id) && ids[0] > p.Id).Count());

//Console.WriteLine(cnn.Get<Person>(p => names.Contains(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => p.Name.Contains("2d")).Count());
//Console.WriteLine(cnn.Get<Person>(p => "ad2".Equals(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => new[] { "ad2", "ad2ddfff" }.Contains(p.Name)).Count());
//Console.WriteLine(cnn.Get<Person>(p => new List<string>(new[] { "ad2", "ad2ddfff" }).Contains(p.Name)).Count());