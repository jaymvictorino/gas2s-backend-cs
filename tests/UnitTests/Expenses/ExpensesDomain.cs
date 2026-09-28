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
    public void Create_Should_Store_Assigned_Values()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

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
    public void Create_EmptyUserId_Throw()
    {
        var userId = Guid.Empty;
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

        var expense = () => Expense.Create(userId, amt, cat, desc, date, time, createdAt);

        expense.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void Create_ZeroOrNegativeAmount_ThrowException(decimal amt)
    {
        var userId = Guid.NewGuid();
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

        var expense = () => Expense.Create(userId, amt, cat, desc, date, time, createdAt);

        expense.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("amount");
    }

    [Fact]
    public void Create_InvalidCategory_ThrowException()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const string desc = "Gift for Liji";
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

        var expense = () =>
            Expense.Create(userId, amt, (ExpenseCategory)999, desc, date, time, createdAt);

        expense.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("category");
    }

    [Fact]
    public void Create_VeryLongDescription_ThrowException()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        var veryLongDesc = new string('x', 501);
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

        var expense = () => Expense.Create(userId, amt, cat, veryLongDesc, date, time, createdAt);

        expense.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Create_TrimDescription_NullDescription()
    {
        var userId = Guid.NewGuid();
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        var date = new DateOnly(2026, 9, 22);
        var time = new TimeOnly(2, 11, 0);
        var createdAt = _fakeTime.GetUtcNow();

        var blank = Expense.Create(userId, amt, cat, "     ", date, time, createdAt);
        var withSpaces = Expense.Create(userId, amt, cat, "     ewan    ", date, time, createdAt);

        blank.Description.Should().BeNull();
        withSpaces.Description.Should().Be("ewan");
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
        var createdAt = _fakeTime.GetUtcNow();

        var expense = Expense.Create(userId, amt, cat, desc, date, time, createdAt);

        const decimal updAmt = 150.00m;
        const ExpenseCategory updCat = ExpenseCategory.Leisure;
        const string updDesc = "New gift for Liji";
        _fakeTime.Advance(TimeSpan.FromHours(1));
        var updatedAt = _fakeTime.GetUtcNow();

        expense.Update(updAmt, updCat, updDesc, date, time, updatedAt);

        expense.Id.Should().NotBe(Guid.Empty);
        expense.UserId.Should().Be(userId);
        expense.Amount.Should().Be(updAmt);
        expense.Category.Should().Be(updCat);
        expense.Description.Should().Be(updDesc);
        expense.Date.Should().Be(date);
        expense.Time.Should().Be(time);
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
