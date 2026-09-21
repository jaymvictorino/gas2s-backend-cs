namespace Expenses.Application.Dto;

public record CreateExpenseRequest(
    Guid Id,
    decimal Amount,
    string Category,
    string Description,
    DateTime Date
);
