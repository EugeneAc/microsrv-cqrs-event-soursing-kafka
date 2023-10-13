using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Query.Infrastructure.DataAccess
{
    public class DatabaseContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;
        public DatabaseContextFactory(Action<DbContextOptionsBuilder> configureContext)
        {
            _configureDbContext = configureContext;
        }

        public DataBaseContext CreateDbContext()
        {
            DbContextOptionsBuilder<DataBaseContext> optionsBuilder = new();
            _configureDbContext(optionsBuilder);

            return new DataBaseContext(optionsBuilder.Options);
        }
    }
}
