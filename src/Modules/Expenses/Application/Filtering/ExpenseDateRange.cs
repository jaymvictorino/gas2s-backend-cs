namespace Expenses.Application.Filtering;

public readonly record struct ExpenseDateRange(DateOnly From, DateOnly To);
