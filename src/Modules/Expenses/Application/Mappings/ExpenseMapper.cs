using Expenses.Application.Dto;
using Expenses.Domain.Entities;

namespace Expenses.Application.Mappings;

public class ExpenseMapper
{
    public static Expense ToEntity(CreateExpenseRequestDto createExpenseRequestDto, Guid userId) =>
        Expense.Create(
            userId,
            createExpenseRequestDto.Amount,
            createExpenseRequestDto.Category,
            createExpenseRequestDto.Description,
            createExpenseRequestDto.Date,
            createExpenseRequestDto.Time,
            DateTimeOffset.UtcNow
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
