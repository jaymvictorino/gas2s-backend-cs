using Expenses.Application.Dto;
using Expenses.Domain.Entities;

namespace Expenses.Application.Mappings;

public static class ExpenseMapper
{
    public static Expense ToEntity(
        Guid userId,
        CreateExpenseRequestDto createExpenseRequestDto,
        DateTimeOffset createdAt
    ) =>
        Expense.Create(
            userId,
            createExpenseRequestDto.Amount,
            createExpenseRequestDto.Category,
            createExpenseRequestDto.Description,
            createExpenseRequestDto.Date,
            createExpenseRequestDto.Time,
            createdAt
        );

    public static ExpenseResponseDto ToDto(Expense expense) =>
        new(
            expense.Id,
            expense.Amount,
            expense.Category,
            expense.Description,
            expense.Date,
            expense.Time
        );
}
