// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.TestModels;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NorthwindQueryNpgsqlFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;

        private readonly NpgsqlTestStore _testStore = NpgsqlNorthwindContext.GetSharedStore();
        private readonly TestSqlLoggerFactory _testSqlLoggerFactory = new TestSqlLoggerFactory();

        public NorthwindQueryNpgsqlFixture()
        {
            _options = BuildOptions();
        }

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => ConfigureOptions(
                new DbContextOptionsBuilder()
                    .EnableSensitiveDataLogging()
                    .UseInternalServiceProvider((additionalServices ?? new ServiceCollection())
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                        .AddSingleton<ILoggerFactory>(_testSqlLoggerFactory)
                        .BuildServiceProvider()))
                .UseNpgsql(
                    ConnectionCreator.CreateConnection( _testStore.Name ),
                    b =>
                    {
                        ConfigureOptions(b);
                        b.ApplyConfiguration();
                    }).Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual void ConfigureOptions(NpgsqlDbContextOptionsBuilder npgsqlDbContextOptionsBuilder)
        {
        }

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            => new NpgsqlNorthwindContext(_options, queryTrackingBehavior);

        public void Dispose() => _testStore.Dispose();

        public override CancellationToken CancelQuery() => _testSqlLoggerFactory.CancelQuery();
    }
}
