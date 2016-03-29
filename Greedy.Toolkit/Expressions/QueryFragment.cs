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
        public ICollection<Column> GroupPart { get; set; }
        public ICollection<OrderCondition> OrderPart { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public QueryFragment()
        {
            this.SelectPart = new List<Column>();
            this.FromPart = new List<Table>();
            this.OrderPart = new List<OrderCondition>();
            this.GroupPart = new List<Column>();
        }

        public int PartsCount(QueryPart parts)
        {
            if ((parts & QueryPart.Select) > 0)
            {
                return SelectPart.Count;
            }

            if ((parts & QueryPart.From) > 0)
            {
                return FromPart.Count;
            }

            if ((parts & QueryPart.Where) > 0)
            {
                return WherePart == null ? 0 : 1;
            }

            if ((parts & QueryPart.GroupBy) > 0)
            {
                return GroupPart.Count;
            }

            if ((parts & QueryPart.OrderBy) > 0)
            {
                return OrderPart.Count;
            }

            if ((parts & QueryPart.Skip) > 0)
            {
                return Skip.Value;
            }

            if ((parts & QueryPart.Take) > 0)
            {
                return Take.Value;
            }

            return 0;
        }

        public bool HasOnlyParts(QueryPart parts)
        {
            var result = true;
            var oppositeResult = true;
            if ((parts & QueryPart.Select) > 0)
            {
                result &= SelectPart.Count > 0;
            }
            else
            {
                oppositeResult &= SelectPart.Count > 0;
            }

            if ((parts & QueryPart.From) > 0)
            {
                result &= FromPart.Count > 0;
            }
            else
            {
                oppositeResult &= FromPart.Count > 0;
            }

            if ((parts & QueryPart.Where) > 0)
            {
                result &= WherePart == null;
            }
            else
            {
                oppositeResult &= WherePart == null;
            }

            if ((parts & QueryPart.GroupBy) > 0)
            {
                result &= GroupPart.Count > 0;
            }
            else
            {
                oppositeResult &= GroupPart.Count > 0;
            }

            if ((parts & QueryPart.OrderBy) > 0)
            {
                result &= OrderPart.Count > 0;
            }
            else
            {
                oppositeResult &= OrderPart.Count > 0;
            }

            if ((parts & QueryPart.Skip) > 0)
            {
                result &= Skip.HasValue;
            }
            else
            {
                oppositeResult &= Skip.HasValue;
            }

            if ((parts & QueryPart.Take) > 0)
            {
                result &= Take.HasValue;
            }
            else
            {
                oppositeResult &= Take.HasValue;
            }

            return result && !oppositeResult;
        }

        public bool HasAnyParts()
        {
            return SelectPart.Count > 0 || FromPart.Count > 0 || WherePart != null || GroupPart.Count > 0 || OrderPart.Count > 0 || Skip.HasValue || Take.HasValue;
        }

        public string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();

            sb.Append("Select ");
            if (SelectPart.Any())
            {
                sb = SelectPart.Aggregate(sb, (s, i) => s.AppendFormat("{0},", i.ToSql(generator)));
                sb.Remove(sb.Length - 1, 1);
            }
            else
            {
                sb.Append("*");
            }
            sb.Append(" From ");
            sb = FromPart.Aggregate(sb, (s, i) => s.AppendFormat("{0} ", i.ToSql(generator)));

            if (WherePart != null)
            {
                sb.AppendFormat("Where {0}", WherePart.ToSql(generator));
            }

            if (GroupPart.Any())
            {
                sb.Append(" Group By ");
                sb = GroupPart.Aggregate(sb, (s, o) => s.AppendFormat("{0},", o.ToSql(generator, false)));
                sb.Remove(sb.Length - 1, 1);
            }

            if (OrderPart.Any())
            {
                sb.Append(" Order By ");
                sb = OrderPart.Aggregate(sb, (s, o) => s.AppendFormat("{0},", o.ToSql(generator)));
                sb.Remove(sb.Length - 1, 1);
            }

            if (Take.HasValue)
            {
                sb.Append(" limit ");
                if (Skip.HasValue)
                {
                    sb.AppendFormat("{0},", Skip.Value);
                }
                sb.Append(Take.Value);
            }

            return sb.ToString();
        }
    }

    [Flags]
    enum QueryPart
    {
        Select = 0x01,
        From = 0x02,
        Where = 0x04,
        GroupBy = 0x08,
        OrderBy = 0x10,
        Skip = 0x20,
        Take = 0x40,
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
            sb.Append(generator.DecorateName(TableName));
            if (!string.IsNullOrEmpty(this.Alias))
                sb.AppendFormat(" AS {0}", Alias);
            return sb.ToString();
        }
    }

    class QueryTable : Table
    {
        public QueryFragment InnerFragment { get; set; }

        public QueryTable(QueryFragment fragment, string alias)
            : base(alias)
        {
            this.InnerFragment = fragment;
        }

        public QueryTable(QueryFragment fragment)
            : base(null)
        {
            this.InnerFragment = fragment;
        }

        public override string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("({0})", InnerFragment.ToSql(generator));
            if (!string.IsNullOrEmpty(this.Alias))
                sb.AppendFormat(" AS {0}", Alias);
            return sb.ToString();
        }
    }

    enum JoinMode
    {
        Default,
        InnerJoin,
        LeftJoin
    }

    class JoinTable : Table
    {
        public Table Table { get; set; }

        public JoinMode JoinMode { get; set; }

        public Condition JoinCondition { get; set; }

        public JoinTable(Table table, string alias)
            : base(alias)
        {
            this.Table = table;
        }

        public JoinTable(Table table)
            : base(null)
        {
            this.Table = table;
        }

        private string GetJoinSql(JoinMode mode)
        {
            if (mode == JoinMode.Default) return ",";
            if (mode == JoinMode.InnerJoin) return "Inner Join";
            if (mode == JoinMode.LeftJoin) return "Left Join";
            else return "";
        }

        public override string ToSql(SqlGenerator generator)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} ", GetJoinSql(JoinMode));
            sb.Append(Table.ToSql(generator));
            if (!string.IsNullOrEmpty(this.Alias))
                sb.AppendFormat(" AS {0}", Alias);
            if (JoinMode != JoinMode.Default)
            {
                sb.AppendFormat(" On {0}", JoinCondition.ToSql(generator));
            }
            return sb.ToString();
        }
    }

    abstract class Column
    {
        public string Alias { get; set; }
        public Type Type { get; set; }
        public abstract string ToSql(SqlGenerator generator, bool withAlias = true);

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

        public override string ToSql(SqlGenerator generator, bool withAlias = true)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(TableName))
                sb.AppendFormat("{0}.", generator.DecorateName(TableName));
            sb.Append(generator.DecorateName(MemberName));
            if (!string.IsNullOrEmpty(this.Alias) && withAlias)
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

        public override string ToSql(SqlGenerator generator, bool withAlias = true)
        {
            var sb = new StringBuilder();
            if (this.Parameters != null && this.Parameters.Any())
                sb.AppendFormat(this.Formatter, this.Parameters.Select(p => p.ToSql(generator)).ToArray());
            else
                sb.Append(this.Formatter);
            if (!string.IsNullOrEmpty(this.Alias) && withAlias)
                sb.AppendFormat(" AS {0}", Alias);
            return sb.ToString();
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

        public override string ToSql(SqlGenerator generator, bool withAlias = true)
        {
            if (!string.IsNullOrEmpty(this.Alias) && withAlias)
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

        public Condition Concat(Condition condition)
        {
            return new GroupCondition() { Relation = " and ", Left = this, Right = condition };
        }
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

    class OrderCondition
    {
        public Column Column { get; set; }
        public string Order { get; set; }

        public OrderCondition(Column column, string order)
        {
            this.Column = column;
            this.Order = order;
        }

        public string ToSql(SqlGenerator generator)
        {
            return string.Format("{0} {1}", Column.ToSql(generator), Order);
        }
    }
}