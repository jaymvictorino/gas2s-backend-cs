namespace Gas2s.IntegrationTests.Expenses;

[Collection(ExpenseDatabaseCollection.Name)]
public class ExpensesInfrastructureTest : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
