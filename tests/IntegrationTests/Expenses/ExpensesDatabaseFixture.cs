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

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
