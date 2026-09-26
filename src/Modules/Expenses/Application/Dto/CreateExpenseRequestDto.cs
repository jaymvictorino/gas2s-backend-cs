using Expenses.Domain.Enums;

namespace Expenses.Application.Dto;

public record CreateExpenseRequestDto(
    decimal Amount,
    ExpenseCategory Category,
    string Description,
    DateTime Date
);
