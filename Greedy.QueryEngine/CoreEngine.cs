using Greedy.Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Greedy.QueryEngine
{
    public class CoreEngine
    {
        public IDbConnectionProvider DbConnectionProvider { get; private set; }
        public ICacheProvider CacheProvider { get; private set; }
        public Profile QueryProfile { get; set; }

        private const string queryConfigureFileName = "/qconf.json";
        private Profile profile;

        //public CoreEngine()
        //{
        //    InitProfile();
        //}

        public CoreEngine(IDbConnectionProvider connectionProvider, ICacheProvider cacheProvider)
        {
            InitProfile();
            this.DbConnectionProvider = connectionProvider;
            this.CacheProvider = cacheProvider;
        }

        public IEnumerable<T> Query<T>(string ruleName, object parameter, object state = null, Action<IEnumerable<T>> action = null)
        {
            IEnumerable<T> result;
            var rule = profile.Rules.First(r => r.Name == ruleName);
            var sqlStatement = profile.SqlStatements.First(s => s.Name == rule.SqlStatement);

            if (!IsNeedCache(rule.Expire))
            {
                //从普通查询中获取 
                result = InternalQuery<T>(sqlStatement, parameter, state);
            }
            else
            {
                //尝试从缓存中获取
                var key = HashParameters(ruleName, parameter, state);
                result = CacheProvider.Get<IEnumerable<T>>(key);
                if (result == null)
                {
                    result = InternalQuery<T>(sqlStatement, parameter, state);
                    SetResultToCache(key, result, rule.Expire);
                }
            }

            if (action != null)
            {
                //如果有事后处理函数，则执行
                action(result);
            }

            return result;
        }

        public PagedResult<T> PagedQuery<T>(string ruleName, object parameter, object state = null, Action<IEnumerable<T>> action = null)
        {
            PagedResult<T> result;
            var rule = profile.Rules.First(r => r.Name == ruleName);
            var sqlStatement = profile.SqlStatements.First(s => s.Name == rule.SqlStatement);
            var countSqlStatement = profile.SqlStatements.First(s => s.Name == rule.CountSqlStatement);

            if (!IsNeedCache(rule.Expire))
            {
                //从普通查询中获取 
                result = new PagedResult<T>()
                {
                    Data = InternalQuery<T>(sqlStatement, parameter, state),
                    Total = InternalCountQuery(countSqlStatement, parameter, state)
                };
            }
            else
            {
                //尝试从缓存中获取
                var key = HashParameters(ruleName, parameter, state);
                result = CacheProvider.Get<PagedResult<T>>(key);
                if (result == null)
                {
                    result = new PagedResult<T>()
                    {
                        Data = InternalQuery<T>(sqlStatement, parameter, state),
                        Total = InternalCountQuery(countSqlStatement, parameter, state)
                    };
                    SetResultToCache(key, result, rule.Expire);
                }
            }

            if (action != null)
            {
                //如果有事后处理函数，则执行
                action(result.Data);
            }

            return result;
        }

        private IEnumerable<T> InternalQuery<T>(SqlStatement statement, object parameter, object state)
        {
            var conneciton = this.DbConnectionProvider.GetConnection(state);
            return conneciton.Query<T>(statement.Sql, parameter);
        }

        private int InternalCountQuery(SqlStatement statement, object parameter, object state)
        {
            var conneciton = this.DbConnectionProvider.GetConnection(state);
            return conneciton.ExecuteScalar<int>(statement.Sql, parameter);
        }

        private bool IsNeedCache(int? expire)
        {
            return CacheProvider != null && expire.HasValue;
        }

        private void SetResultToCache(string key, object obj, int? expire)
        {
            //如果有缓存要求，将结果进行缓存
            if (expire.Value > 0)
                CacheProvider.Set(key, obj, expire.Value);
            else
                CacheProvider.Set(key, obj);
        }

        private string HashParameters(string ruleName, object parameter, object state = null)
        {
            var byts = new List<byte>();
            byts.AddRange(GetBytesFromObject(ruleName));
            if (parameter != null)
                byts.AddRange(GetBytesFromObject(parameter));
            if (state != null)
                byts.AddRange(GetBytesFromObject(state));
            return Hash(byts.ToArray());
        }

        private string Hash(byte[] byts)
        {
            var md5 = new MD5CryptoServiceProvider();
            var code = md5.ComputeHash(byts);
            var hashStr = new StringBuilder();
            hashStr = code.Aggregate(hashStr, (sb, b) => sb.AppendFormat("{0:x2}", b));
            return hashStr.ToString();
        }

        private byte[] GetBytesFromObject(object obj)
        {
            if (obj == null)
                return null;
            using (var memory = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memory, obj);
                memory.Position = 0;
                var read = new byte[memory.Length];
                memory.Read(read, 0, read.Length);
                return read;
            }
        }

        private void InitProfile()
        {
            using (var st = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + queryConfigureFileName))
            {
                var str = st.ReadToEnd();
                profile = JsonConvert.DeserializeObject<Profile>(str);
            }
        }
    }
}
