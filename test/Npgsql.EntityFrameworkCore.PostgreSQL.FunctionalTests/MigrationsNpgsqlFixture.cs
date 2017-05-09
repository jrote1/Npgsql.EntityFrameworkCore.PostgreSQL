using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class MigrationsNpgsqlFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;

        public MigrationsNpgsqlFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseNpgsql(ConnectionCreator.CreateConnection( nameof( MigrationsNpgsqlTest ) ) ).Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_options);

        public override EmptyMigrationsContext CreateEmptyContext() => new EmptyMigrationsContext(_options);
    }
}
