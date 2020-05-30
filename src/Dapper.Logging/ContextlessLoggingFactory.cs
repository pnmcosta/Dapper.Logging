using System;
using System.Data.Common;
using Dapper.Logging.Configuration;
using Dapper.Logging.Hooks;
using Microsoft.Extensions.Logging;

namespace Dapper.Logging
{
    public class ContextlessLoggingFactory : IDbConnectionFactory
    {
        private readonly LoggingHook<Empty> _hooks;
        private readonly WrappedConnectionFactory<Empty> _factory;
        
        public ContextlessLoggingFactory(
            ILoggerFactory loggerFactory, 
            DbLoggingConfiguration config, 
            Func<DbConnection> factory)
        {
            _hooks = new LoggingHook<Empty>(loggerFactory, config);
            _factory = new WrappedConnectionFactory<Empty>(factory);
        }
        
        public DbConnection CreateConnection() => 
            _factory.CreateConnection(_hooks, Empty.Object);
    }
}