using Expenses.Domain.Enums;

namespace Expenses.Application.Dto;

public record ExpenseResponseDto(
    Guid Id,
    decimal Amount,
    ExpenseCategory Category,
    string? Description,
    DateOnly Date,
    TimeOnly Time
);
