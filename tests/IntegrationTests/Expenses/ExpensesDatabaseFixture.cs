using Expenses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Gas2s.IntegrationTests.Expenses;

public sealed class ExpensesDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSql = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("expense_module_test")
        .WithUsername("test_user")
        .WithPassword("test_pass")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _postgreSql.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgreSql.StartAsync();

        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public ExpenseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ExpenseDbContext(options);
    }
}
