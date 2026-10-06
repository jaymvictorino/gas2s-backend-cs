using Expenses.Domain.Enums;

namespace Expenses.Application.Dto;

public record UpdateExpenseRequestDto(
    decimal Amount,
    ExpenseCategory Category,
    string? Description,
    DateOnly Date,
    TimeOnly Time
);
