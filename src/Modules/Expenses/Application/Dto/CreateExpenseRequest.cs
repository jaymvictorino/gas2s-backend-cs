namespace Expenses.Application.Dto;

public record CreateExpenseRequest(
    decimal Amount,
    string Category,
    string Description,
    DateTime Date
);
