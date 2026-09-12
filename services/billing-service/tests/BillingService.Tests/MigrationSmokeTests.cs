using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BillingService.Tests;

public sealed class MigrationSmokeTests
{
    [Fact]
    public async Task FreshDatabaseMigratesAndCurrentModelCanBeQueried()
    {
        var connectionString = Environment.GetEnvironmentVariable("MIGRATION_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString)) return;
        var database = "voyara_migration_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE {database}", admin))
            await create.ExecuteNonQueryAsync();
        try
        {
            var isolated = new NpgsqlConnectionStringBuilder(connectionString) { Database = database, Pooling = false };
            await using var db = new BillingDbContext(new DbContextOptionsBuilder<BillingDbContext>()
                .UseNpgsql(isolated.ConnectionString).Options);
            await db.Database.MigrateAsync();
            await db.Database.MigrateAsync();
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.Empty(await db.Subscriptions.ToListAsync());
            Assert.Empty(await db.CommercialPackages.ToListAsync());
            Assert.Empty(await db.TenantStripeLinks.ToListAsync());
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE {database} WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
