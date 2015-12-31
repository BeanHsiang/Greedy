using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class QueryFragment
    {
        public ICollection<Column> SelectPart { get; set; }
        public Condition WherePart { get; set; }
        public ICollection<Table> FromPart { get; set; }

        public QueryFragment()
        {
            this.SelectPart = new List<Column>();
            this.FromPart = new List<Table>();
        }

        public string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();

            sb.Append("Select ");
            if (SelectPart.Any())
            {
                sb = SelectPart.Aggregate(sb, (s, i) => s.AppendFormat("{0} ", i.ToSql(generator)));
            }
            else
            {
                sb.Append("* ");
            }
            sb.Append("From ");
            sb = FromPart.Aggregate(sb, (s, i) => s.AppendFormat("{0} ", i.ToSql(generator)));

            if (WherePart != null)
            {
                sb.AppendFormat("Where {0}", WherePart.ToSql(generator));
            }
            return sb.ToString();
        }
    }

    abstract class Table
    {
        public string Alias { get; set; }
        public Type Type { get; set; }
        public abstract string ToSql(SqlGenerator generator);

        public Table(string alias)
        {
            this.Alias = alias;
        }
    }

    class SingleTable : Table
    {
        public string TableName { get; set; }

        public SingleTable(string tableName, string alias)
            : base(alias)
        {
            this.TableName = tableName;
        }

        public SingleTable(string tableName)
            : base(null)
        {
            this.TableName = tableName;
        }

        public override string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();
            sb.Append(TableName);
            if (!string.IsNullOrEmpty(this.Alias))
                sb.AppendFormat(" AS {0}", Alias);
            return sb.ToString();
        }
    }

    abstract class Column
    {
        public string Alias { get; set; }
        public Type Type { get; set; }
        public abstract string ToSql(SqlGenerator generator);

        public Column(string alias)
        {
            this.Alias = alias;
        }
    }

    class MemberColumn : Column
    {
        public string MemberName { get; set; }
        public string TableName { get; set; }

        public MemberColumn(string memberName, string tableName, string alias)
            : base(alias)
        {
            this.MemberName = memberName;
            this.TableName = tableName;
        }

        public MemberColumn(string memberName, string tableName)
            : base(null)
        {
            this.MemberName = memberName;
            this.TableName = tableName;
        }

        public override string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(TableName))
                sb.AppendFormat("{0}.", generator.DecorateName(TableName));
            sb.Append(generator.DecorateName(MemberName));
            if (!string.IsNullOrEmpty(this.Alias))
                sb.AppendFormat(" AS {0}", Alias);
            return sb.ToString();
        }
    }

    class FunctionColumn : Column
    {
        public string Formatter { get; set; }
        public ICollection<Column> Parameters { get; private set; }

        public FunctionColumn(string alias)
            : base(alias)
        {
            this.Parameters = new List<Column>();
        }

        public FunctionColumn()
            : base(null)
        {
            this.Parameters = new List<Column>();
        }

        public void Add(Column column)
        {
            this.Parameters.Add(column);
        }

        public override string ToSql(SqlGenerator generator)
        {
            return string.Format(this.Formatter, this.Parameters.Select(p => p.ToSql(generator)).ToArray());
        }
    }

    class ParameterColumn : Column
    {
        public string MemberName { get; private set; }

        public ParameterColumn(string name)
            : base(null)
        {
            this.MemberName = name;
        }

        public ParameterColumn(string name, string alias)
            : base(alias)
        {
            this.MemberName = name;
        }

        public override string ToSql(SqlGenerator generator)
        {
            if (!string.IsNullOrEmpty(this.Alias))
            {
                return string.Format("{0} AS {1}", MemberName, Alias);
            }
            return MemberName;
        }
    }

    abstract class Condition
    {
        public string Relation { get; set; }
        public abstract string ToSql(SqlGenerator generator);
    }

    class SingleCondition : Condition
    {
        public Column Left { get; set; }
        public Column Right { get; set; }

        public override string ToSql(SqlGenerator generator)
        {
            if (Left != null)
                return string.Format("{0} {1} {2}", Left.ToSql(generator), Relation, Right.ToSql(generator));
            return Right.ToSql(generator);
        }
    }

    class GroupCondition : Condition
    {
        public Condition Left { get; set; }
        public Condition Right { get; set; }

        public override string ToSql(SqlGenerator generator)
        {
            if (Left != null)
                return string.Format("({0}) {1} ({2})", Left.ToSql(generator), Relation, Right.ToSql(generator));
            return Right.ToSql(generator);
        }
    }
}