using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
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

    private readonly DateTimeOffset _nowUtc = new(2026, 9, 27, 14, 0, 0, TimeSpan.Zero);

    public ExpensesInfrastructureTest(ExpensesDatabaseFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _transaction = await _context.Database.BeginTransactionAsync();
        _repo = new ExpenseRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _context.DisposeAsync();
    }

    private Expense NewExpense(
        Guid userId,
        decimal amount = 100m,
        ExpenseCategory category = ExpenseCategory.Groceries,
        string? description = "test",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return Expense.Create(
            userId,
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(_nowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(_nowUtc.DateTime),
            _nowUtc
        );
    }
}
