using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.QueryEngine
{
    public class Profile
    {
        public IList<SqlStatement> SqlStatements { get; set; }
        public IList<Rule> Rules { get; set; }
    }

    public class SqlStatement
    {
        public string Name { get; set; }
        public string Sql { get; set; }
    }

    public class Rule
    {
        public string Name { get; set; }
        public string SqlStatementName { get; set; }
        public string Filter { get; set; }
        public int? Expire { get; set; }
        public string DependRuleName { get; set; }
    }
}