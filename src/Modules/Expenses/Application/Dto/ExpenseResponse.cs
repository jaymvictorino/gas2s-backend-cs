namespace Expenses.Application.Dto;

public record ExpenseResponse(decimal Amount, string Category, string Description, DateTime Date);
