using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.QueryEngine
{
    public interface IDbConnectionProvider
    {
        IDbConnection GetConnection();
        IDbConnection GetConnection(object state);
    }

    class DbConnectionProvider : IDbConnectionProvider
    {
        public IDbConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public IDbConnection GetConnection(object state)
        {
            return GetConnection();
        }
    }
}
