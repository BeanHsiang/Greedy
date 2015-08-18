using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    static partial class SqlMapper
    {
        private static TypeHandler typeHandler;

        private static TypeHandler GetTypeHandler(IDbConnection cnn)
        {
            if (typeHandler == null)
            {
                typeHandler = new TypeHandler(cnn);
            }
            return typeHandler;
        }

        public static int Insert<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetInsertSql<T>(param), param, transaction, commandTimeout, commandType);
        }

        public static long InsertWithIdentity<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.ExecuteScalar<long>(GetTypeHandler(cnn).GetInsertSqlWithIdentity<T>(param), param, transaction, commandTimeout, commandType);
        }

        public static int Update<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetUpdateSql<T>(param), param, transaction, commandTimeout, commandType);
        }
    }
}
