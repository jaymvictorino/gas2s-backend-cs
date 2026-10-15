using Expenses.Application.Dto;
using Expenses.Application.Filtering;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using Expenses.Infrastructure.Data;
using Expenses.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

    private ExpenseDateRange NewFilter(
        ExpenseRangePreset range,
        DateOnly? from = null,
        DateOnly? to = null
    )
    {
        return ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(range, from, to),
            DateOnly.FromDateTime(_nowUtc.DateTime)
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

        var result = await _repo.GetAllAsync(_userA, NewFilter(ExpenseRangePreset.Week));

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

        var result = await _repo.GetAllAsync(_userA, NewFilter(ExpenseRangePreset.Month));

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

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_PersistAndReturnsExpense()
    {
        var expense = NewExpense(
            _userA,
            amount: 127.00m,
            category: ExpenseCategory.Clothing,
            description: "Gift"
        );

        var persisted = await _repo.CreateAsync(expense);

        persisted.Id.Should().Be(expense.Id);

        var fromDb = await _context.Expenses.SingleAsync(e => e.Id == expense.Id);
        fromDb.Amount.Should().Be(expense.Amount);
        fromDb.Category.Should().Be(expense.Category);
        fromDb.Description.Should().Be(expense.Description);
    }

    // ---------- UpdateAsync ----------

    [Fact]
    public async Task UpdateAsync_PersistsChanges_ReturnsUpdatedExpense()
    {
        var expense = NewExpense(_userA);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var tracked = await _repo.GetByIdAsync(_userA, expense.Id);
        tracked!.Update(
            127.00m,
            ExpenseCategory.Clothing,
            "Updated",
            tracked.Date,
            tracked.Time,
            _nowUtc.AddHours(1)
        );

        var updated = await _repo.UpdateAsync(tracked);

        updated.Id.Should().Be(expense.Id);
        updated.Amount.Should().Be(127.00m);
        updated.Category.Should().Be(ExpenseCategory.Clothing);
        updated.Description.Should().Be("Updated");

        var fromDb = await _context.Expenses.FindAsync(expense.Id);
        fromDb!.Amount.Should().Be(127.00m);
        fromDb.Category.Should().Be(ExpenseCategory.Clothing);
        fromDb.Description.Should().Be("Updated");
    }

    // ---------- DeleteAsync ----------

    [Fact]
    public async Task DeleteAsync_RemovesExpense_ReturnsTrue()
    {
        var expense = NewExpense(_userA);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var result = await _repo.DeleteAsync(expense);

        result.Should().BeTrue();

        var fromDb = await _context.Expenses.FindAsync(expense.Id);
        fromDb.Should().BeNull();
    }
}
