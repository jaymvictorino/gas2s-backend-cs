using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesDomainTest
{
    private readonly FakeTimeProvider _fakeTime = new(
        new DateTimeOffset(2026, 9, 27, 14, 0, 0, TimeSpan.Zero)
    );
    private readonly DateTimeOffset _nowUtc = new(2026, 9, 27, 14, 0, 0, TimeSpan.Zero);

    private readonly Guid _userId = Guid.NewGuid();

    private readonly decimal _amount = 100m;
    private readonly ExpenseCategory _category = ExpenseCategory.Clothing;
    private readonly string _description = "test";
    private readonly DateOnly _date = new(2026, 9, 22);
    private readonly TimeOnly _time = new(2, 11, 0);

    private Expense Expense =>
        Expense.Create(_userId, _amount, _category, _description, _date, _time, _nowUtc);

    [Fact]
    public void Create_Should_Store_Assigned_Values()
    {
        var expense = Expense.Create(
            _userId,
            _amount,
            _category,
            _description,
            _date,
            _time,
            _nowUtc
        );

        expense.Id.Should().NotBe(Guid.Empty);
        expense.UserId.Should().Be(_userId);
        expense.Amount.Should().Be(_amount);
        expense.Category.Should().Be(_category);
        expense.Description.Should().Be(_description);
        expense.Date.Should().Be(_date);
        expense.Time.Should().Be(_time);
        expense.CreatedAtUtc.Should().Be(_nowUtc);
        expense.UpdatedAtUtc.Should().Be(_nowUtc);
    }

    [Fact]
    public void Create_EmptyUserId_Throw()
    {
        var expense = () =>
            Expense.Create(Guid.Empty, _amount, _category, _description, _date, _time, _nowUtc);

        expense.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void Create_ZeroOrNegativeAmount_ThrowException(decimal amount)
    {
        var expense = () =>
            Expense.Create(_userId, amount, _category, _description, _date, _time, _nowUtc);
        expense.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("amount");
    }

    [Fact]
    public void Create_InvalidCategory_ThrowException()
    {
        var expense = () =>
            Expense.Create(
                _userId,
                _amount,
                (ExpenseCategory)999,
                _description,
                _date,
                _time,
                _nowUtc
            );

        expense.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("category");
    }

    [Fact]
    public void Create_VeryLongDescription_ThrowException()
    {
        var expense = () =>
            Expense.Create(
                _userId,
                _amount,
                _category,
                new string('x', 501),
                _date,
                _time,
                _nowUtc
            );

        expense.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Create_TrimDescription_NullDescription()
    {
        var blank = Expense.Create(_userId, _amount, _category, "     ", _date, _time, _nowUtc);
        var withSpaces = Expense.Create(
            _userId,
            _amount,
            _category,
            "     ewan    ",
            _date,
            _time,
            _nowUtc
        );

        blank.Description.Should().BeNull();
        withSpaces.Description.Should().Be("ewan");
    }

    [Fact]
    public void Update_ExistingId_UpdateExpense()
    {
        var expense = Expense;

        const decimal updAmt = 150.00m;
        const ExpenseCategory updCat = ExpenseCategory.Leisure;
        const string updDesc = "New gift for Liji";

        var updatedAt = _nowUtc.AddHours(1);

        expense.Update(updAmt, updCat, updDesc, _date, _time, updatedAt);

        expense.Id.Should().NotBe(Guid.Empty);
        expense.UserId.Should().Be(_userId);
        expense.Amount.Should().Be(updAmt);
        expense.Category.Should().Be(updCat);
        expense.Description.Should().Be(updDesc);
        expense.Date.Should().Be(_date);
        expense.Time.Should().Be(_time);
        expense.CreatedAtUtc.Should().Be(_nowUtc);
        expense.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void Update_ZeroOrNegativeAmount_ThrowException(decimal amount)
    {
        var updExp = () =>
            Expense.Update(amount, _category, _description, _date, _time, _nowUtc.AddHours(1));

        updExp.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("amount");
    }

    [Fact]
    public void Update_InvalidCategory_ThrowException()
    {
        var updExp = () =>
            Expense.Update(
                _amount,
                (ExpenseCategory)999,
                _description,
                _date,
                _time,
                _nowUtc.AddHours(1)
            );

        updExp.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("category");
    }

    [Fact]
    public void Update_VeryLongDescription_ThrowException()
    {
        var updExp = () =>
            Expense.Update(
                _amount,
                _category,
                new string('x', 501),
                _date,
                _time,
                _nowUtc.AddHours(1)
            );

        updExp.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Update_TrimDescription_NullDescription()
    {
        var expense = Expense;

        var expense2 = Expense;

        _fakeTime.Advance(TimeSpan.FromHours(1));

        expense.Update(_amount, _category, "   Gift    ", _date, _time, _nowUtc);

        expense2.Update(_amount, _category, "       ", _date, _time, _nowUtc);

        expense.Description.Should().Be("Gift");
        expense2.Description.Should().BeNull();
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
