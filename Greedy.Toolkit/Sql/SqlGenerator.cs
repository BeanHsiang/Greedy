using System;
using System.Collections.Generic;
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
            var columns = mapper.AllMembers.Where(m => !m.IsIdentity).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0},", DecorateName(item.ColumnName)); });
            var parameters = mapper.AllMembers.Where(m => !m.IsIdentity).Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0},", DecorateParameter(item.Name)); });
            sb.AppendFormat("Insert into {0}({1}) Values({2})", DecorateName(mapper.TableName), columns.Remove(columns.Length - 1, 1), parameters.Remove(parameters.Length - 1, 1));
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
            sb.AppendFormat("Update {0} Set {1} Where {2}", DecorateName(mapper.TableName), GetConditionSql(mapper.AllMembers.Except(mapper.GetKeyMembers(true))), GetConditionSql(mapper.GetKeyMembers(true)));
            return sb.ToString();
        }

        public string GetDeleteSql(ITypeMapper mapper, string whereSql)
        {
            return string.Format("Delete {0} Where {1}", DecorateName(mapper.TableName), whereSql);
        }

        public string GetDeleteSql(ITypeMapper mapper, string alias, string whereSql)
        {
            return string.Format("Delete {0} From {1} Where {2}", alias, DecorateName(mapper.TableName, alias), whereSql);
        }

        public string GetConditionSql(IEnumerable<MemberMapper> memberMappers)
        {
            var sb = new StringBuilder();
            var conditions = memberMappers.Aggregate(new StringBuilder(), (s, item) => { return s.AppendFormat("{0}={1},", DecorateName(item.ColumnName), DecorateParameter(item.Name)); });
            return conditions.Remove(conditions.Length - 1, 1).ToString();
        }

        public string DecorateName(string name, string alias = null)
        {
            var sb = new StringBuilder();
            if (name.First() != SqlDialect.LeftQuote && name.Last() != SqlDialect.RightQuote)
            {
                sb.AppendFormat("{0}{1}{2}", SqlDialect.LeftQuote, name, SqlDialect.RightQuote);
            }
            else
            {
                sb.Append(name);
            }

            if (!string.IsNullOrEmpty(alias))
            {
                sb.AppendFormat(" As {0}", alias);
            }
            return sb.ToString();
        }

        public string DecorateParameter(string paramName)
        {
            if (paramName.First() != SqlDialect.ParameterPrefix)
            {
                return string.Format("{0}{1}", SqlDialect.ParameterPrefix, paramName);
            }
            return paramName;
        }
    }
}