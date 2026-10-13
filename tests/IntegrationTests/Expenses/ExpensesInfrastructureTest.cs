using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using Expenses.Infrastructure.Data;
using Expenses.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gas2s.IntegrationTests.Expenses;

[Collection(ExpenseDatabaseCollection.Name)]
public class ExpensesInfrastructureTest : IAsyncLifetime
{
    private readonly ExpensesDatabaseFixture _fixture;
    private ExpenseDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;
    private ExpenseRepository _repo = null!;

    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();
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

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_GetUserAExpenses_ReturnsOnlyExpensesForUserA()
    {
        _context.Expenses.AddRange(
            NewExpense(_userA, description: "A1"),
            NewExpense(_userA, description: "A2"),
            NewExpense(_userB, description: "B1")
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetAllAsync(_userA);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(expense => expense.UserId == _userA);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsExpensesInDescendingOrder()
    {
        _context.Expenses.AddRange(
            NewExpense(_userA, date: new DateOnly(2026, 9, 26), description: "middle"),
            NewExpense(_userA, description: "newest-early"),
            NewExpense(
                _userA,
                time: TimeOnly.FromDateTime(_nowUtc.AddHours(2).DateTime),
                description: "newest-late"
            ),
            NewExpense(_userA, date: new DateOnly(2026, 9, 1), description: "oldest")
        );
        await _context.SaveChangesAsync();

        var result = await _repo.GetAllAsync(_userA);

        result
            .Select(expense => expense.Description)
            .Should()
            .ContainInOrder("newest-late", "newest-early", "middle", "oldest");
    }

    [Fact]
    public async Task GetAllAsync_UserHasNoExpense_ReturnsEmptyList()
    {
        var result = await _repo.GetAllAsync(_userA);

        result.Should().BeEmpty();
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExpenseOwnedByUser_ReturnsExpense()
    {
        var expense = NewExpense(_userA);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(_userA, expense.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(expense.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExpenseNotOwnedByUser_ReturnsNull()
    {
        var expense = NewExpense(_userA);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(_userB, expense.Id);

        result.Should().BeNull();
    }
}
