using System;
using System.Data.Common;
using Dapper.Logging.Configuration;
using Dapper.Logging.Hooks;
using Microsoft.Extensions.Logging;

namespace Dapper.Logging
{
    internal class LoggingHook<T> : ISqlHooks<T>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DbLoggingConfiguration _config;
        private readonly Func<DbConnection, object> _connectionProjector;

        public LoggingHook(ILoggerFactory loggerFactory, DbLoggingConfiguration config)
        {
            _loggerFactory = loggerFactory;
            _config = config;
            _connectionProjector = _config.ConnectionProjector ?? (_ => Empty.Object);
        }

        public void ConnectionOpened(DbConnection connection, T context, double elapsedMs) =>
            GetLogger(connection).Log(
                _config.LogLevel, 
                _config.OpenConnectionMessage,
                elapsedMs,
                context,
                _connectionProjector(connection));

        public void ConnectionClosed(DbConnection connection, T context, double elapsedMs) =>
            GetLogger(connection).Log(
                _config.LogLevel, 
                _config.CloseConnectionMessage, 
                elapsedMs,
                context,
                _connectionProjector(connection));

        public void CommandExecuted(DbCommand command, T context, double elapsedMs) =>
            GetLogger(command).Log(
                _config.LogLevel, 
                _config.ExecuteQueryMessage, 
                command.CommandText,
                command.GetParameters(hideValues: !_config.LogSensitiveData),
                elapsedMs,
                context,
                _connectionProjector(command.Connection));

        private ILogger GetLogger(object value)
        {
            return _loggerFactory.CreateLogger(value.GetType());
        }
    }
}