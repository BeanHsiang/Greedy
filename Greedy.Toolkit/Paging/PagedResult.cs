using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Paging
{
    public class Result // : ServiceResult<object>
    {
        public bool Status { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public Result(bool status, string code, string message)
        {
            this.Status = status;
            this.Code = code;
            this.Message = message;
        }

        public Result(bool status, string message)
        {
            this.Status = status;
            this.Message = message;
        }
    }

    public class Result<T>
    {
        public bool Status { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public Result() { }

        public Result(bool status, string code, string message)
        {
            this.Status = status;
            this.Code = code;
            this.Message = message;
        }

        public Result(bool status, string message)
        {
            this.Status = status;
            this.Message = message;
        }
    }

    public class PagedResult<T> : Result<IEnumerable<T>>
    {
        public int Total { get; set; }
        public int OldPageIndex { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }

        public PagedResult() : base() { }

        public PagedResult(bool status, string code, string message)
            : base(status, code, message)
        {
        }

        public PagedResult(bool status, string message)
            : base(status, message)
        {
        }
    }
}
