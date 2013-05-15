using System;
using System.Net;
using System.Data.Linq;

namespace Knock
{
    public class DbStorage : DataContext
    {
        public DbStorage(string connectionString)
            : base(connectionString)
        {
        }
        public Table<Db> user
        {
            get
            {
                return this.GetTable<Db>();
            }
        }
    }
}
