using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Paging
{
    public class PagedCondition
    {
        public IDictionary<string, dynamic> Filter { get; set; }
        public IDictionary<string, QueryOrder> Order { get; set; }
        public PagedParameter Pageparameter { get; set; }

        public PagedCondition()
        {
            this.Pageparameter = new PagedParameter();
        }

        public string ToLimitSql(Dictionary<string, dynamic> input)
        {
            input.Add("mysql_offset", Pageparameter.PageIndex - 1);
            input.Add("mysql_limit", Pageparameter.PageSize);
            return string.Format("limit @mysql_offset,@mysql_limit");
        }

        public string ToWhereSql(Func<IDictionary<string, dynamic>, string> func)
        {
            return func(Filter);
        }

        public string ToOrderSql(Func<IDictionary<string, QueryOrder>, string> func)
        {
            return func(Order);
        }
    }

    public enum QueryOrder
    {
        ASC,
        DESC
    }
}
