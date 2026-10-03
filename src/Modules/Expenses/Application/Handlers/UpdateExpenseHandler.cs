using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class UpdateExpenseHandler
{
    private readonly IExpenseRepository _repo;

    public UpdateExpenseHandler(IExpenseRepository repo) => _repo = repo;

    public async Task<ExpenseResponseDto?> UpdateExpenseAsync(
        Guid userId,
        Guid id,
        CreateExpenseRequestDto expenseRequestDto
    )
    {
        var expense = await _repo.GetByIdAsync(userId, id);

        if (expense is null)
            return null;

        expense.Update(
            expenseRequestDto.Amount,
            expenseRequestDto.Category,
            expenseRequestDto.Description,
            expenseRequestDto.Date,
            expenseRequestDto.Time,
            DateTimeOffset.UtcNow
        );
        var updatedExpense = await _repo.UpdateAsync(userId, expense);

        return ExpenseMapper.ToDto(updatedExpense);
    }
}
