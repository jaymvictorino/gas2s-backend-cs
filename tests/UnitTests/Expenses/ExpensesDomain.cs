using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesDomain
{
    private readonly FakeTimeProvider _fakeTime = new(new DateTimeOffset(2026, 9, 27, 14, 0 ,0, TimeSpan.Zero));
    
    [Fact]
    public void Expense_Should_Store_Assigned_Values()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateTime(2026, 9, 22, 2, 11, 0, DateTimeKind.Utc);
        var createdAt = DateTime.UtcNow;

        var expense = Expense.Create(userId, amt, cat, desc, date, createdAt);

        expense.Id.Should().NotBe(Guid.Empty);
        expense.UserId.Should().Be(userId);
        expense.Amount.Should().Be(amt);
        expense.Category.Should().Be(cat);
        expense.Description.Should().Be(desc);
        expense.Date.Should().Be(date);
        expense.CreatedAtUtc.Should().Be(createdAt);
        expense.UpdatedAtUtc.Should().Be(createdAt);
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
