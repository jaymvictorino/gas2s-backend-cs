using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Application.Validators;
using FluentValidation.TestHelper;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpenseFilterRequestDtoValidatorTests
{
    private readonly ExpenseFilterRequestDtoValidator _filterRequestDtoValidator;

    public ExpenseFilterRequestDtoValidatorTests() =>
        _filterRequestDtoValidator = new ExpenseFilterRequestDtoValidator();

    private ExpenseFilterRequestDto NewFilter(
        ExpenseRangePreset range,
        DateOnly? from = null,
        DateOnly? to = null
    ) => new(range, from, to);

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
            ExpenseRangePreset.Week,
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
        var filter = NewFilter(ExpenseRangePreset.Week, date, date);

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
}
