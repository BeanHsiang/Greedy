using Greedy.Dapper;
using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class QueryExpressionParser : ExpressionVisitor
    {
        private IDbConnection connection;
        private ExpressionVisitorContext context;

        internal QueryExpressionParser(IDbConnection connection)
        {
            this.connection = connection;
            this.context = new ExpressionVisitorContext(connection) { UseTableAlias = true };
        }

        internal QueryExpressionParser(IDbConnection connection, ExpressionVisitorContext context)
        {
            this.connection = connection;
            this.context = context;
        }

        internal void Parse(Expression expression)
        {
            this.Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "Where":
                    this.Visit(node.Arguments[0]);
                    if (this.context.IsQueryResultChanged)
                    {
                        var oldContext = this.context;
                        this.context = oldContext.CopyTo();
                        this.context.Fragment.FromPart.Add(new QueryTable(oldContext.WrapToFragment(), this.context.GetTableAlias(oldContext.ExportType)));
                    }
                    ParseWhereExpresson(node.Arguments[1]);
                    break;
                case "Select":
                    this.Visit(node.Arguments[0]);
                    var selectVisitor = new SelectExpressionVisitor(context);
                    selectVisitor.Visit(node.Arguments[1]);
                    this.context.Fragment.SelectPart = selectVisitor.Columns;
                    break;
                case "Skip":
                    this.Visit(node.Arguments[0]);
                    this.context.Fragment.Skip = (int)(node.Arguments[1] as ConstantExpression).Value;
                    break;
                case "Take":
                    this.Visit(node.Arguments[0]);
                    this.context.Fragment.Take = (int)(node.Arguments[1] as ConstantExpression).Value;
                    break;
                case "OrderBy":
                case "OrderByDescending":
                case "ThenBy":
                case "ThenByDescending":
                    this.Visit(node.Arguments[0]);
                    var orderVisitor = new MemberExpressionVisitor(context);
                    orderVisitor.Visit(node.Arguments[1]);
                    this.context.Fragment.OrderPart.Add(new OrderCondition(orderVisitor.Column,
                        node.Method.Name == "OrderBy" || node.Method.Name == "ThenBy" ? "asc" : "desc"));
                    break;
                case "Any":
                case "Count":
                    var parser = new QueryExpressionParser(this.connection, this.context.CopyTo());
                    parser.Visit(node.Arguments[0]);
                    this.context.Fragment.FromPart.Add(new QueryTable(parser.context.WrapToFragment(), this.context.GetTableAlias(parser.ExportType)));
                    if (node.Arguments.Count > 1)
                        ParseWhereExpresson(node.Arguments[1]);
                    this.context.Fragment.SelectPart.Add(new FunctionColumn() { Formatter = "Count(*)" });
                    break;
                case "GroupJoin":
                    ParseJoinExpression(node, JoinMode.LeftJoin);
                    break;
                case "Join":
                    ParseJoinExpression(node, JoinMode.InnerJoin);
                    break;
                case "SelectMany":
                    ParseSelectManyExpression(node);
                    break;
                case "Predicate":
                    if (node.Arguments[0].Type == typeof(IDbConnection))
                    {
                        var elementType = node.Type.GetGenericArguments()[0];
                        this.context.ExportType = this.context.ImportType = elementType;
                        var typeMapper = TypeMapperCache.GetTypeMapper(elementType);
                        this.context.Fragment.FromPart.Add(new SingleTable(typeMapper.TableName, this.context.GetTableAlias(elementType)));
                    }
                    break;
                case "DefaultIfEmpty":
                    break;
                default:
                    break;
            }
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var elementType = typeof(T);
            if (elementType.IsGenericType && elementType.GetGenericArguments().Last() == typeof(bool))
            {
                var visitor = new WhereExpressionVisitor(context);
                visitor.Visit(node);
                context.Fragment.WherePart = visitor.Condition;
            }
            else
            {
                Visit(node.Body);
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitor = new BinaryExpressionVisitor(context);
            visitor.Visit(node);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.GetGenericTypeDefinition() == typeof(DataQuery<>))
            {
                var elementType = node.Type.GetGenericArguments()[0];
                this.context.ExportType = this.context.ImportType = elementType;
                var typeMapper = TypeMapperCache.GetTypeMapper(elementType);
                this.context.Fragment.FromPart.Add(new SingleTable(typeMapper.TableName, this.context.GetTableAlias(elementType)));
            }
            return node;
        }

        internal string ToSql()
        {
            return context.ToSql();
        }

        private void ParseWhereExpresson(Expression node)
        {
            var visitor = new WhereExpressionVisitor(context);
            visitor.Visit(node);
            if (this.context.Fragment.WherePart == null)
                this.context.Fragment.WherePart = visitor.Condition;
            else
                this.context.Fragment.WherePart = this.context.Fragment.WherePart.Concat(visitor.Condition);
        }

        private void ParseJoinExpression(Expression node, JoinMode mode)
        {
            var methodExpression = node as MethodCallExpression;
            Visit(methodExpression.Arguments[0]);
            if (!this.context.Fragment.HasOnlyParts(QueryPart.From))
            {
                var oldContext = this.context;
                this.context = oldContext.CopyTo();
                var queryTable = new QueryTable(oldContext.WrapToFragment(), this.context.GetTableAlias(oldContext.ExportType));
                this.context.Fragment.FromPart.Add(queryTable);
            }

            var joinContext = this.context.CopyTo();
            var joinParser = new QueryExpressionParser(this.connection, joinContext);
            joinParser.Parse(methodExpression.Arguments[1]);

            Table table;
            if (joinContext.Fragment.HasOnlyParts(QueryPart.From) && joinContext.Fragment.FromPart.Count == 1)
            {
                table = joinContext.Fragment.FromPart.First();
                table.Alias = this.context.GetTableAlias(joinContext.ExportType);
            }
            else
            {
                table = new QueryTable(joinContext.WrapToFragment(), this.context.GetTableAlias(joinContext.ExportType));
            }

            var joinVisitor = new JoinExpressionVisitor(this.context);
            joinVisitor.Parse(methodExpression.Arguments[2], methodExpression.Arguments[3], methodExpression.Arguments[4]);
            this.context.Fragment.FromPart.Add(new JoinTable(table) { JoinMode = mode, JoinCondition = joinVisitor.Condition });
        }

        private void ParseSelectManyExpression(Expression node)
        {
            var methodExpression = node as MethodCallExpression;
            Visit(methodExpression.Arguments[0]);
            if (!this.context.Fragment.HasOnlyParts(QueryPart.From))
            {
                var oldContext = this.context;
                this.context = oldContext.CopyTo();
                var queryTable = new QueryTable(oldContext.WrapToFragment(), this.context.GetTableAlias(oldContext.ExportType));
                this.context.Fragment.FromPart.Add(queryTable);
            }

            var joinContext = this.context.CopyTo();
            var joinParser = new QueryExpressionParser(this.connection, joinContext);
            joinParser.Parse(methodExpression.Arguments[1]);

            Table table = null;
            if (joinContext.Fragment.HasOnlyParts(QueryPart.From) && joinContext.Fragment.PartsCount(QueryPart.From) == 1)
            {
                table = joinContext.Fragment.FromPart.First();
                table.Alias = this.context.GetTableAlias(joinContext.ExportType);
            }
            else if (joinContext.Fragment.HasAnyParts())
            {
                table = new QueryTable(joinContext.WrapToFragment(), this.context.GetTableAlias(joinContext.ExportType));
            }

            var joinVisitor = new JoinExpressionVisitor(this.context);
            joinVisitor.Parse(methodExpression.Arguments[2]);

            if (table != null)
                this.context.Fragment.FromPart.Add(new JoinTable(table) { JoinMode = JoinMode.Default });
        }

        internal IDictionary<string, object> Parameters { get { return context.Parameters; } }

        internal IDictionary<Type, string> TableNames { get { return context.TableNames; } }

        internal Type ExportType { get { return context.ExportType; } }
    }
}
