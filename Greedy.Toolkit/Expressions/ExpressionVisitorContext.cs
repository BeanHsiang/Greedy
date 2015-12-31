using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class ExpressionVisitorContext
    {
        public SqlGenerator Generator { get; private set; }
        public QueryFragment Fragment { get; private set; }
        public IDictionary<string, object> Parameters { get; private set; }
        public IDictionary<Type, string> TableNames { get; private set; }

        internal bool UseTableAlias { get; set; }
        //internal bool UserColumnAlias { get; set; }

        internal ExpressionVisitorContext(IDbConnection connection)
        {
            this.Generator = new SqlGenerator(connection);
            this.Fragment = new QueryFragment();
            this.Parameters = new Dictionary<string, object>();
            this.TableNames = new Dictionary<Type, string>();
        }

        internal ExpressionVisitorContext(SqlGenerator generator)
        {
            this.Generator = generator;
            this.Fragment = new QueryFragment();
            this.Parameters = new Dictionary<string, object>();
            this.TableNames = new Dictionary<Type, string>();
        }

        public string AddParameter(object obj)
        {
            var name = string.Format("p{0}", Parameters.Count);
            this.Parameters.Add(name, obj);
            return Generator.DecorateParameter(name);
        }

        public string GetTableAlias(Type type)
        {
            if (!UseTableAlias)
            {
                return string.Empty;
            }
            if (TableNames.ContainsKey(type))
            {
                return TableNames[type];
            }
            var name = string.Format("tb{0}", TableNames.Count);
            this.TableNames.Add(type, name);
            return name;
        }

        public string ToSql()
        {
            return this.Fragment.ToSql(this.Generator);
        }
    }
}