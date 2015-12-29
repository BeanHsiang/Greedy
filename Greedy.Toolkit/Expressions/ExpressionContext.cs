using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class ExpressionContext
    {
        public IDictionary<string, dynamic> Parameters { get; private set; }
        public IDictionary<ITypeMapper, string> Alias { get; private set; }
        public TypeHandler TypeHandler { get; private set; }

        internal ExpressionContext(TypeHandler typeHandler)
        {
            TypeHandler = typeHandler;
            Parameters = new Dictionary<string, dynamic>();
            Alias = new Dictionary<ITypeMapper, string>();
        }

        internal ExpressionContext(TypeHandler typeHandler, IDictionary<string, dynamic> parameters)
        {
            TypeHandler = typeHandler;
            Parameters = parameters;
            Alias = new Dictionary<ITypeMapper, string>();
        }

        public string AddParameter(string name, dynamic value)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Format("param{0}", Parameters.Count);
            }
            Parameters.Add(name, value);
            var decorateParamName = TypeHandler.SqlGenerator.DecorateParameter(name);
            return decorateParamName;
        }

        public void AddAlias(Type type, string alias = "")
        {
            var typeMapper = TypeMapperCache.GetTypeMapper(type);
            if (Alias.Keys.Any(k => k.Code == typeMapper.Code)) return;
            if (string.IsNullOrEmpty(alias))
            {
                alias = string.Format("tb{0}", Alias.Count);
            }
            Alias.Add(typeMapper, alias);
        }

        public string GetAlias(ITypeMapper typeMapper)
        {
            string alias = null;
            Alias.TryGetValue(typeMapper, out alias);
            return alias;
        }
    }
}
