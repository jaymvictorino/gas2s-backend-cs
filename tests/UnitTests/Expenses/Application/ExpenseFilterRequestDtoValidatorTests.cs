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
}
