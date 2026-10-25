using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Validators;
using FluentValidation.Results;
using Gas2s.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gas2s.Api.Controllers;

[ApiController]
[Route("/api/expenses")]
public class ExpensesController : ControllerBase
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);

    private readonly GetExpensesHandler _get;

    private readonly ExpenseFilterRequestDtoValidator _filterValidator;

    public ExpensesController(
        GetExpensesHandler get,
        ExpenseFilterRequestDtoValidator filterValidator
    )
    {
        _get = get;
        _filterValidator = filterValidator;
    }

    // ModelState.AddModelError helper for validation errors
    private void AddErrors(ValidationResult validation)
    {
        foreach (var failure in validation.Errors)
        {
            ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDto>>> GetExpenses(
        [FromQuery] string? filter = "month",
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null
    )
    {
        if (!ExpenseRangeParser.TryParse(filter, out var range))
        {
            ModelState.AddModelError(
                "filter",
                $"Unknown filter {filter}. Valid values are: week, month, 3months, custom."
            );
            return ValidationProblem(ModelState);
        }

        var filterDto = new ExpenseFilterRequestDto(range, from, to);

        var validation = await _filterValidator.ValidateAsync(filterDto);
        if (!validation.IsValid)
        {
            AddErrors(validation);
            return ValidationProblem(ModelState);
        }

        var expenses = await _get.GetExpensesAsync(Guid.NewGuid(), filterDto, Today);

        return Ok(expenses);
    }
}
