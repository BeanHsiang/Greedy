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
        public string SqlStatement { get; set; }
        public string CountSqlStatement { get; set; }
        public int? Expire { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
    }
}