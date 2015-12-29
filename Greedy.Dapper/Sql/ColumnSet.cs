using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Greedy.Dapper;

namespace Greedy.Toolkit.Sql
{
    public class ColumnSet<T>
    {
        internal IDbConnection Connection { get; private set; }
        IDictionary<Expression<Func<T, object>>, object> paramInput;
        Expression<Func<T, bool>> condition = null;

        public ColumnSet(IDbConnection connection)
        {
            this.Connection = connection;
            paramInput = new Dictionary<Expression<Func<T, object>>, object>();
        }

        public ColumnSet<T> Set(Expression<Func<T, object>> expr, object value)
        {
            paramInput.Add(expr, value);
            return this;
        }

        public ColumnSet<T> Where(Expression<Func<T, bool>> expr)
        {
            this.condition = expr;
            return this;
        }

        public int Update(IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return this.Connection.Update<T>(condition, paramInput, transaction, commandTimeout);
        }
    }
}