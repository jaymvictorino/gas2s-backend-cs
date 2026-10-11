using Expenses.Infrastructure.Data;
using Expenses.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gas2s.IntegrationTests.Expenses;

[Collection(ExpenseDatabaseCollection.Name)]
public class ExpensesInfrastructureTest : IAsyncLifetime
{
    private readonly ExpensesDatabaseFixture _fixture;
    private ExpenseDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private ExpenseRepository _repo = null!;

    public ExpensesInfrastructureTest(ExpensesDatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _repo = new ExpenseRepository(_context);
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
