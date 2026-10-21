using Expenses.Application.Dto;
using Expenses.Application.Validators;
using Expenses.Domain.Enums;
using FluentValidation.TestHelper;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpensesApplicationValidatorsTests
{
    private readonly ExpensesApplicationTest _application;

    private readonly CreateExpenseRequestDtoValidator _createExpenseRequestDtoValidator;
    private readonly UpdateExpenseRequestDtoValidator _updateExpenseRequestDtoValidator;

    public ExpensesApplicationValidatorsTests()
    {
        _application = new ExpensesApplicationTest();
        _createExpenseRequestDtoValidator = new CreateExpenseRequestDtoValidator();
        _updateExpenseRequestDtoValidator = new UpdateExpenseRequestDtoValidator();
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
}
