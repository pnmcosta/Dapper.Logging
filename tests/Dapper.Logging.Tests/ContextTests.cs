using System;
using System.Data.Common;
using System.Linq;
using Dapper.Logging.Configuration;
using Dapper.Logging.Tests.Infra;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dapper.Logging.Tests
{
    public class ContextTests
    {
        [Fact]
        public void Should_pass_extra_context_to_log_messages()
        {
            var loggerFactory = new TestLoggerFactory();
            var innerConnection = Substitute.For<DbConnection>();
            var services = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddDbConnectionFactoryWithCtx<Context>(
                prv => innerConnection,
                x => x.WithLogLevel(LogLevel.Information),
                ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbConnectionFactory<Context>>();
            
            var ctx = new Context{Id = Guid.NewGuid()};
            var connection = factory.CreateConnection(ctx);
            connection.Open();
            
            //assert
            innerConnection.Received().Open();
            loggerFactory.Loggers.Values.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages[0].State["@context"].Should().Be(ctx);
        }
        
        [Fact]
        public void Should_pass_anonymous_context_to_log_messages()
        {
            var loggerFactory = new TestLoggerFactory();
            var innerConnection = Substitute.For<DbConnection>();
            var services = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddDbConnectionFactoryWithCtx<object>(
                prv => innerConnection,
                x => x.WithLogLevel(LogLevel.Information),
                ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbConnectionFactory<object>>();
            
            var ctx = new {Id = Guid.NewGuid()};
            var connection = factory.CreateConnection(ctx);
            connection.Open();
            
            //assert
            innerConnection.Received().Open();
            loggerFactory.Loggers.Values.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages[0].State["@context"].Should().Be(ctx);
        }
        
        [Fact]
        public void Should_pass_connection_to_log_messages()
        {
            var loggerFactory = new TestLoggerFactory();
            var innerConnection = Substitute.For<DbConnection>();
            innerConnection.DataSource.Returns("source123");
            var services = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory);

            services.AddDbConnectionFactoryWithCtx<Context>(
                prv => innerConnection,
                x => x.WithLogLevel(LogLevel.Information)
                    .WithConnectionProjector(c => new {c.DataSource}),
                ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbConnectionFactory<Context>>();
            
            var ctx = new Context{Id = Guid.NewGuid()};
            var connection = factory.CreateConnection(ctx);
            connection.Open();
            
            //assert
            innerConnection.Received().Open();
            loggerFactory.Loggers.Values.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages.Should().HaveCount(1);
            loggerFactory.Loggers.Values.First().Messages[0].State.Should().ContainKey("@connection");
            loggerFactory.Loggers.Values.First().Messages[0].State["@connection"].Should()
                        .BeEquivalentTo(new {DataSource = "source123"});
            
        }
    }

    public class Context
    {
        public Guid Id { get; set; }
    }
}