namespace Expenses.Application.Dto;

public record ExpenseResponseDto(
    Guid Id,
    decimal Amount,
    string Category,
    string Description,
    DateTime Date
);
