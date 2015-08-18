using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Greedy.Toolkit.Sql
{
    class SqlGenerator
    {
        public ISqlDialect SqlDialect { get; private set; }

        internal SqlGenerator(IDbConnection connection)
        {
            SqlDialect = InitSqlDialect(connection);
        }

        private ISqlDialect InitSqlDialect(IDbConnection connection)
        {
            //string name = connection == null ? null : connection.GetType().Name;
            //if (string.Equals(name, "npgsqlconnection", StringComparison.OrdinalIgnoreCase)) return postgres;

            return new MySqlDialect();
        }

        public string GetInsertSql(ITypeMapper mapper)
        {
            var sb = new StringBuilder();
            var columns = mapper.AllMembers.Where(m => !m.IsIdentity).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0}{1}{2},", SqlDialect.LeftQuote, item.ColumnName, SqlDialect.RightQuote); });
            var parameters = mapper.AllMembers.Where(m => !m.IsIdentity).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0}{1},", SqlDialect.ParameterPrefix, item.Name); });
            sb.AppendFormat("Insert into {0}{1}{2}({3}) Values({4})", SqlDialect.LeftQuote, mapper.TableName, SqlDialect.RightQuote, columns.Remove(columns.Length - 1, 1), parameters.Remove(parameters.Length - 1, 1));
            return sb.ToString();
        }

        public string GetInsertSqlWithIdentity(ITypeMapper mapper)
        {
            return string.Format("{0};{1}", GetInsertSql(mapper), SqlDialect.GetIdentitySql());
        }

        public string GetUpdateSql(ITypeMapper mapper)
        {
            var sb = new StringBuilder();
            //.Where(m => !m.IsIdentity) 非主键不允许有自增长列
            var columns = mapper.AllMembers.Except(mapper.GetKeyMembers(true)).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0}{1}{2}={3}{4},", SqlDialect.LeftQuote, item.ColumnName, SqlDialect.RightQuote, SqlDialect.ParameterPrefix, item.Name); });
            var conditions = mapper.GetKeyMembers(true).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0}{1}{2}={3}{4},", SqlDialect.LeftQuote, item.ColumnName, SqlDialect.RightQuote, SqlDialect.ParameterPrefix, item.Name); });
            sb.AppendFormat("Update {0}{1}{2} Set {3} Where {4}", SqlDialect.LeftQuote, mapper.TableName, SqlDialect.RightQuote, columns.Remove(columns.Length - 1, 1), conditions.Remove(conditions.Length - 1, 1));
            return sb.ToString();
        }
    }
}