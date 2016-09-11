using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Paging
{
    public class PagedParameter
    {
        const int DefaultPageIndex = 1;
        const int DefaultPageSize = 12;
        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public int PageCount { get; private set; }

        public int TotalCount
        {
            set
            {
                var total = value;
                this.PageCount = total <= 0 ? 0 :
                    total % this.PageSize == 0 ? total / this.PageSize : total / this.PageSize + 1;
                this.PageIndex = this.PageCount <= 0 ? DefaultPageIndex :
                    this.PageIndex > this.PageCount ? this.PageCount : this.PageIndex;
            }
        }

        public PagedParameter()
            : this(DefaultPageIndex, DefaultPageSize)
        {
        }

        public PagedParameter(int pageIndex, int pageSize)
        {
            this.PageIndex = pageIndex < DefaultPageIndex ? DefaultPageIndex : pageIndex;
            this.PageSize = pageSize < 0 ? DefaultPageSize : pageSize;
        }
    }
}
