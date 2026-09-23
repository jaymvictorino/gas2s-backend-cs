namespace Expenses.Application.Dto;

public record ExpenseResponse(
    Guid Id,
    decimal Amount,
    string Category,
    string Description,
    DateTime Date
);
