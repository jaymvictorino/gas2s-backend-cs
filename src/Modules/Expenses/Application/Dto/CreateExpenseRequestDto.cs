namespace Expenses.Application.Dto;

public record CreateExpenseRequestDto(
    decimal Amount,
    string Category,
    string Description,
    DateTime Date
);
