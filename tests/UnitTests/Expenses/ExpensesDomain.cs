using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesDomain
{
    private readonly FakeTimeProvider _fakeTime = new(
        new DateTimeOffset(2026, 9, 27, 14, 0, 0, TimeSpan.Zero)
    );

    [Fact]
    public void Expense_Should_Store_Assigned_Values()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime;

        var expense = Expense.Create(userId, amt, cat, desc, date, time, createdAt);

        expense.Id.Should().NotBe(Guid.Empty);
        expense.UserId.Should().Be(userId);
        expense.Amount.Should().Be(amt);
        expense.Category.Should().Be(cat);
        expense.Description.Should().Be(desc);
        expense.Date.Should().Be(date);
        expense.Time.Should().Be(time);
        expense.CreatedAtUtc.Should().Be(createdAt);
        expense.UpdatedAtUtc.Should().Be(createdAt);
    }

    [Fact]
    public void Update_ExistingId_UpdateExpense()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime;

        var expense = Expense.Create(userId, amt, cat, desc, date, time, createdAt);

        const decimal updAmt = 150.00m;
        const ExpenseCategory updCat = ExpenseCategory.Leisure;
        const string updDesc = "New gift for Liji";
        _fakeTime.Advance(TimeSpan.FromHours(1));
        var updatedAt = _fakeTime.GetUtcNow();
        expense.Update(updAmt, updCat, updDesc, date, time, _fakeTime));
    }

    [Fact]
    public void ExpenseCategory_Should_Contain_Expected_Enums()
    {
        var categories = Enum.GetValues<ExpenseCategory>();

        categories
            .Should()
            .BeEquivalentTo([
                ExpenseCategory.Groceries,
                ExpenseCategory.Leisure,
                ExpenseCategory.Electronics,
                ExpenseCategory.Utilities,
                ExpenseCategory.Clothing,
                ExpenseCategory.Health,
                ExpenseCategory.Others,
            ]);
    }
}
