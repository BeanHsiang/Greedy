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
            var conneciton = this.DbConnectionProvider.GetConnection(state);
            var rule = profile.Rules.First(r => r.Name == ruleName);
            IEnumerable<T> result;
            string key = "";
            if (CacheProvider != null && rule.Expire.HasValue)
            {
                key = HashParameters(ruleName, parameter, state);
                var obj = CacheProvider.Get<IEnumerable<T>>(key);
                if (obj != null)
                    return obj;
            }

            if (!string.IsNullOrEmpty(rule.DependRuleName))
            {
                result = Query<T>(rule.DependRuleName, parameter, state);
            }
            else
            {
                var sqlStatementName = rule.SqlStatementName;
                var sqlStatement = profile.SqlStatements.First(s => s.Name == sqlStatementName);
                result = conneciton.Query<T>(sqlStatement.Sql, parameter);
            }

            if (action != null)
            {
                action(result);
            }

            if (CacheProvider != null && rule.Expire.HasValue)
            {
                if (rule.Expire.Value > 0)
                    CacheProvider.Set(key, result, rule.Expire.Value);
                else
                    CacheProvider.Set(key, result);
            }

            return result;
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
