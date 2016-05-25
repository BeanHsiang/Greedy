using Greedy.Dapper;
using Greedy.QueryEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Greedy.Test
{
    [TestClass]
    public class QueryEngineTest
    {
        static MySqlProvider prov;
        Random rand = new Random();


        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            prov = new MySqlProvider();
        }

        [ClassCleanup]
        public static void Clean()
        {
            prov.Dispose();

        }

        class MySqlProvider : IDbConnectionProvider, IDisposable
        {
            private MySqlConnection con;

            public IDbConnection GetConnection()
            {
                var conStr = "server=LocalHost;User Id=root;Pwd=greedyint;database=test";
                con = new MySqlConnection(conStr);
                return con;
            }

            public IDbConnection GetConnection(object state)
            {
                return GetConnection();
            }

            public void Dispose()
            {
                con.Dispose();
            }
        }

        [TestMethod]
        public void TestCoreEngineQuery()
        {
            var core = new CoreEngine(prov, null);
            var result = core.Query<Person>("myperson", new { Age = 30 });
            Assert.AreNotEqual(0, result.Count(), "查询引擎失败");
        }
    }
}
