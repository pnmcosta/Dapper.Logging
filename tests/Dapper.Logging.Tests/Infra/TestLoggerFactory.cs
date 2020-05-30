using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dapper.Logging.Tests.Infra
{
    public class TestLoggerFactory : ILoggerFactory, IDisposable
    {
        public Dictionary<string, TestLogger> Loggers = new Dictionary<string, TestLogger>();

        public void AddProvider(ILoggerProvider provider)
        {
            // do nothing TestLogger's handle their own state .Messages
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (!Loggers.ContainsKey(categoryName))
                Loggers.Add(categoryName, new TestLogger(categoryName));
            return Loggers[categoryName];
        }

        public void Dispose()
        {
            Loggers.Clear();
        }
    }
}
