using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
        public Type ImportType { get; set; }
        public Type ExportType { get; set; }
        public ICollection<Tuple<Type, string, Column>> TempColumnMappers { get; private set; }
        internal bool UseTableAlias { get; set; }
        //internal bool UserColumnAlias { get; set; }

        internal ExpressionVisitorContext(IDbConnection connection)
        {
            this.Generator = new SqlGenerator(connection);
            this.Fragment = new QueryFragment();
            this.Parameters = new Dictionary<string, object>();
            this.TableNames = new Dictionary<Type, string>();
            this.TempColumnMappers = new List<Tuple<Type, string, Column>>();
        }

        internal ExpressionVisitorContext(SqlGenerator generator)
        {
            this.Generator = generator;
            this.Fragment = new QueryFragment();
            this.Parameters = new Dictionary<string, object>();
            this.TableNames = new Dictionary<Type, string>();
            this.TempColumnMappers = new List<Tuple<Type, string, Column>>();
        }

        private ExpressionVisitorContext(SqlGenerator generator, IDictionary<string, object> parameters)
        {
            this.Generator = generator;
            this.Fragment = new QueryFragment();
            this.Parameters = parameters;
            this.TableNames = new Dictionary<Type, string>();
            this.TempColumnMappers = new List<Tuple<Type, string, Column>>();
        }

        private ExpressionVisitorContext(SqlGenerator generator, IDictionary<string, object> parameters, ICollection<Tuple<Type, string, Column>> tempColumnMappers)
        {
            this.Generator = generator;
            this.Fragment = new QueryFragment();
            this.Parameters = parameters;
            this.TableNames = new Dictionary<Type, string>();
            this.TempColumnMappers = tempColumnMappers;
        }

        public string AddParameter(object obj)
        {
            var name = string.Format("p{0}", Parameters.Count);
            this.Parameters.Add(name, obj);
            return Generator.DecorateParameter(name);
        }

        public void AddTempColumnMapper(Tuple<Type, string, Column> tempColumnMapper)
        {
            this.TempColumnMappers.Add(tempColumnMapper);
        }

        public Column GetMappedColumn(Type type, string memberName)
        {
            var tempMapper = this.TempColumnMappers.FirstOrDefault(m => m.Item1 == type && m.Item2 == memberName);

            return tempMapper == null ? null : tempMapper.Item3;
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
            var name = string.Format("t{0}", TableNames.Count);
            this.TableNames.Add(type, name);
            return name;
        }

        public string ToSql()
        {
            return WrapToFragment().ToSql(this.Generator);
        }

        public ExpressionVisitorContext CopyTo()
        {
            return new ExpressionVisitorContext(this.Generator, this.Parameters) { UseTableAlias = true };
        }

        public QueryFragment WrapToFragment()
        {
            //if (this.Fragment.SelectPart == null)
            //{
            //    this.Fragment.SelectPart = new List<Column>();
            //}
            if (this.Fragment.SelectPart.Count == 0)
            {
                var members = this.ExportType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var typeMapper = TypeMapperCache.GetTypeMapper(this.ExportType);
                var tableName = GetTableAlias(this.ExportType);
                for (var i = 0; i < members.Length; i++)
                {
                    var member = members[i];
                    var tempColumn = this.GetMappedColumn(this.ExportType, member.Name);
                    if (tempColumn == null)
                    {
                        var columnMapper = typeMapper.AllMembers.SingleOrDefault(m => m.Name == member.Name);
                        this.Fragment.SelectPart.Add(new MemberColumn(columnMapper.ColumnName, tableName, member.Name) { Type = member.ReflectedType });
                    }
                    else
                    {
                        tempColumn.Alias = member.Name;
                        this.Fragment.SelectPart.Add(tempColumn);
                    }
                }
            }
            return this.Fragment;
        }

        public bool IsQueryResultChanged { get { return this.Fragment.SelectPart.Count > 0; } } //ImportType != ExportType
    }
}