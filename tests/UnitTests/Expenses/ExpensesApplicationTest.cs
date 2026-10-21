using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Application.Mappings;
using Expenses.Application.Validators;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesApplicationTest
{
    protected internal DateTimeOffset NowUtc = new(2026, 9, 27, 14, 0, 0, TimeSpan.Zero);

    protected internal Guid UserId = Guid.NewGuid();

    protected internal ExpenseFilterRequestDto Filter = new(ExpenseRangePreset.Week, null, null);

    private readonly CreateExpenseRequestDtoValidator _createExpenseRequestDtoValidator;
    private readonly UpdateExpenseRequestDtoValidator _updateExpenseRequestDtoValidator;

    public ExpensesApplicationTest()
    {
        _createExpenseRequestDtoValidator = new CreateExpenseRequestDtoValidator();
        _updateExpenseRequestDtoValidator = new UpdateExpenseRequestDtoValidator();
    }

    protected internal Expense NewExpense(
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
            date ?? DateOnly.FromDateTime(NowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(NowUtc.DateTime),
            NowUtc
        );
    }

    protected internal T NewExpenseRequestDto<T>(
        Func<decimal, ExpenseCategory, string, DateOnly, TimeOnly, T> factory,
        decimal amount = 127.00m,
        ExpenseCategory category = ExpenseCategory.Clothing,
        string description = "Gift",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return factory(
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(NowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(NowUtc.DateTime)
        );
    }

    // ---------- ExpenseMapper.ToEntity ----------

    [Fact]
    public void ToEntity_ConvertCreateExpenseRequestDto_ReturnsExpense()
    {
        var expense = ExpenseMapper.ToEntity(
            UserId,
            NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new CreateExpenseRequestDto(amount, category, description, date, time)
            ),
            NowUtc
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(UserId);
    }

    // ---------- ExpenseMapper.ToDto ----------

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expResDto = ExpenseMapper.ToDto(NewExpense(UserId));

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().NotBeEmpty();
    }

    // ---------- CreateExpenseRequestDtoValidator ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void CreateExpenseRequestDtoValidator_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(zeroOrNegativeAmount);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount must be positive.");
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(12.3456)]
    [InlineData(123.4567)]
    public void CreateExpenseRequestDtoValidator_AmountMoreThanTwoDecimalPlaces_ValidationError(
        decimal amount
    )
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount cannot have more than 2 decimal places.");
    }

    [Fact]
    public void CreateExpenseRequestDtoValidator_CategoryNotInExpenseCategory_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, _, description, date, time) =>
                new CreateExpenseRequestDto(amount, (ExpenseCategory)999, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Category)
            .WithErrorMessage("Invalid expense category.");
    }

    [Fact]
    public void CreateExpenseRequestDtoValidator_DescriptionLengthAbove500_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, category, _, date, time) =>
                new CreateExpenseRequestDto(amount, category, new string('x', 501), date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }

    // ---------- UpdateExpenseRequestDtoValidator ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void UpdateExpenseRequestDtoValidator_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new UpdateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(zeroOrNegativeAmount);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount must be positive.");
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(12.3456)]
    [InlineData(123.4567)]
    public void UpdateExpenseRequestDtoValidator_AmountMoreThanTwoDecimalPlaces_ValidationError(
        decimal amount
    )
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new UpdateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount cannot have more than 2 decimal places.");
    }

    [Fact]
    public void UpdateExpenseRequestDtoValidator_CategoryNotInExpenseCategory_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, _, description, date, time) =>
                new UpdateExpenseRequestDto(amount, (ExpenseCategory)999, description, date, time)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Category)
            .WithErrorMessage("Invalid expense category.");
    }

    [Fact]
    public void UpdateExpenseRequestDtoValidator_DescriptionLengthAbove500_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, category, _, date, time) =>
                new UpdateExpenseRequestDto(amount, category, new string('x', 501), date, time)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }
}
