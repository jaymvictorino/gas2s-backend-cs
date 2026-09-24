using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesDomainTest
{
    [Fact]
    public void Expense_Should_Store_Assigned_Values()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateTime(2026, 9, 22, 2, 11, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2026, 9, 22, 2, 13, 0, DateTimeKind.Utc);
        var updatedAt = DateTime.Now;

        var expense = new Expense
        {
            Id = id,
            UserId = userId,
            Amount = amt,
            Category = cat,
            Description = desc,
            Date = date,
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = updatedAt,
        };

        expense.Id.Should().Be(id);
        expense.UserId.Should().Be(userId);
        expense.Amount.Should().Be(amt);
        expense.Category.Should().Be(cat);
        expense.Description.Should().Be(desc);
        expense.Date.Should().Be(date);
        expense.CreatedAtUtc.Should().Be(createdAt);
        expense.UpdatedAtUtc.Should().Be(updatedAt);
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
