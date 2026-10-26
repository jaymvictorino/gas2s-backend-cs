using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Validators;
using FluentValidation.Results;
using Gas2s.Api.Common;
using Gas2s.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace Gas2s.Api.Controllers;

[ApiController]
[Route("/api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ICurrentUser _user;

    private static DateOnly Today => DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);

    private readonly GetExpensesHandler _get;
    private readonly GetExpenseByIdHandler _getById;

    private readonly ExpenseFilterRequestDtoValidator _filterValidator;

    public ExpensesController(
        ICurrentUser user,
        GetExpensesHandler get,
        GetExpenseByIdHandler getById,
        ExpenseFilterRequestDtoValidator filterValidator
    )
    {
        _user = user;
        _get = get;
        _getById = getById;
        _filterValidator = filterValidator;
    }

    // UserId Helper
    private bool TryGetUserId(out Guid userId)
    {
        if (_user.UserId != null)
        {
            userId = _user.UserId.Value;
            return true;
        }

        userId = Guid.Empty;
        return false;
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
        if (!TryGetUserId(out var userId))
            return Unauthorized();

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

        var expenses = await _get.GetExpensesAsync(userId, filterDto, Today);

        return Ok(expenses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetExpenseById(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var expense = await _getById.GetExpenseByIdAsync(userId, id);

        return expense is null ? NotFound() : Ok(expense);
    }
}
