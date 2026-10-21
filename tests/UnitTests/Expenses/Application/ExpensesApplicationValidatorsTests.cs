using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Application.Validators;
using Expenses.Domain.Enums;
using FluentValidation.TestHelper;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpensesApplicationValidatorsTests
{
    private readonly ExpensesApplicationTest _application;

    private readonly CreateExpenseRequestDtoValidator _createExpenseRequestDtoValidator;
    private readonly ExpenseFilterRequestDtoValidator _filterRequestDtoValidator;
    private readonly UpdateExpenseRequestDtoValidator _updateExpenseRequestDtoValidator;

    public ExpensesApplicationValidatorsTests()
    {
        _application = new ExpensesApplicationTest();
        _createExpenseRequestDtoValidator = new CreateExpenseRequestDtoValidator();
        _filterRequestDtoValidator = new ExpenseFilterRequestDtoValidator();
        _updateExpenseRequestDtoValidator = new UpdateExpenseRequestDtoValidator();
    }

    private ExpenseFilterRequestDto NewFilter(
        ExpenseRangePreset range,
        DateOnly? from = null,
        DateOnly? to = null
    ) => new(range, from, to);

    // ---------- CreateExpenseRequestDtoValidator ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void CreateExpenseRequestDtoValidator_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
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
        var zeroOrNegativeAmount = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
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
        var expenseRequestDto = _application.NewExpenseRequestDto(
            (amount, category, _, date, time) =>
                new UpdateExpenseRequestDto(amount, category, new string('x', 501), date, time)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }

    // ---------- ExpenseFilterRequestDtoValidator ----------

    [Theory]
    [InlineData(ExpenseRangePreset.Week)]
    [InlineData(ExpenseRangePreset.Month)]
    [InlineData(ExpenseRangePreset.ThreeMonths)]
    public void ExpenseFilterRequestDtoValidator_NonCustomRange_NoErrors(ExpenseRangePreset range)
    {
        var result = _filterRequestDtoValidator.TestValidate(NewFilter(range));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_NonCustomWithValidDates_NoErrors()
    {
        var filter = NewFilter(
            ExpenseRangePreset.Week,
            new DateOnly(2026, 9, 27),
            new DateOnly(2026, 9, 21)
        );

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomWithValidDates_NoErrors()
    {
        var filter = NewFilter(
            ExpenseRangePreset.Custom,
            new DateOnly(2026, 9, 21),
            new DateOnly(2026, 9, 27)
        );

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomWithEqualDates_NoErrors()
    {
        var date = new DateOnly(2026, 9, 21);
        var filter = NewFilter(ExpenseRangePreset.Custom, date, date);

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomMissingFromDate_ValidationError()
    {
        var filter = NewFilter(ExpenseRangePreset.Custom, null, new DateOnly(2026, 9, 27));

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result
            .ShouldHaveValidationErrorFor(dto => dto.From)
            .WithErrorMessage("From date required.");
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomMissingToDate_ValidationError()
    {
        var filter = NewFilter(ExpenseRangePreset.Custom, new DateOnly(2026, 9, 21));

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(dto => dto.To).WithErrorMessage("To date required.");
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomMissingBothDates_ValidationErrors()
    {
        var filter = NewFilter(ExpenseRangePreset.Custom);

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result
            .ShouldHaveValidationErrorFor(dto => dto.From)
            .WithErrorMessage("From date required.");
        result.ShouldHaveValidationErrorFor(dto => dto.To).WithErrorMessage("To date required.");
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_CustomFromDateAfterToDate_ValidationErrors()
    {
        var filter = NewFilter(
            ExpenseRangePreset.Custom,
            new DateOnly(2026, 9, 27),
            new DateOnly(2026, 9, 21)
        );

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result
            .ShouldHaveValidationErrorFor(dto => dto.From)
            .WithErrorMessage("From date must be before the To date.");
        result
            .ShouldHaveValidationErrorFor(dto => dto.To)
            .WithErrorMessage("To date must be after the From date.");
    }

    [Fact]
    public void ExpenseFilterRequestDtoValidator_InvalidExpenseRangePreset_ValidationError()
    {
        var filter = NewFilter((ExpenseRangePreset)999);

        var result = _filterRequestDtoValidator.TestValidate(filter);

        result
            .ShouldHaveValidationErrorFor(dto => dto.Range)
            .WithErrorMessage("Invalid ExpenseRangePreset value.");
    }
}
