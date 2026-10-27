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
    private readonly CreateExpenseHandler _create;
    private readonly UpdateExpenseHandler _update;
    private readonly DeleteExpenseHandler _delete;

    private readonly CreateExpenseRequestDtoValidator _createValidator;
    private readonly ExpenseFilterRequestDtoValidator _filterValidator;
    private readonly UpdateExpenseRequestDtoValidator _updateValidator;

    public ExpensesController(
        ICurrentUser user,
        GetExpensesHandler get,
        GetExpenseByIdHandler getById,
        CreateExpenseHandler create,
        UpdateExpenseHandler update,
        DeleteExpenseHandler delete,
        CreateExpenseRequestDtoValidator createValidator,
        ExpenseFilterRequestDtoValidator filterValidator,
        UpdateExpenseRequestDtoValidator updateValidator
    )
    {
        _user = user;
        _get = get;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
        _createValidator = createValidator;
        _filterValidator = filterValidator;
        _updateValidator = updateValidator;
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
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpenseResponseDto>> GetExpenseById(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var expense = await _getById.GetExpenseByIdAsync(userId, id);

        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateExpense(
        [FromBody] CreateExpenseRequestDto expenseRequestDto
    )
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var validation = await _createValidator.ValidateAsync(expenseRequestDto);
        if (!validation.IsValid)
        {
            AddErrors(validation);
            return ValidationProblem(ModelState);
        }

        var expense = await _create.CreateExpenseAsync(userId, expenseRequestDto);

        return CreatedAtAction(nameof(GetExpenseById), new { id = Guid.NewGuid() }, expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExpense(
        Guid id,
        [FromBody] UpdateExpenseRequestDto updateDto
    )
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var validation = await _updateValidator.ValidateAsync(updateDto);
        if (!validation.IsValid)
        {
            AddErrors(validation);
            return ValidationProblem(ModelState);
        }

        var updated = await _update.UpdateExpenseAsync(userId, id, updateDto);

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        return await _delete.DeleteExpenseAsync(userId, id) ? NoContent() : NotFound();
    }
}
