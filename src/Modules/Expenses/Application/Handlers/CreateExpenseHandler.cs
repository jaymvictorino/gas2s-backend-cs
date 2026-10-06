using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class CreateExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public CreateExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<ExpenseResponseDto> CreateExpenseAsync(
        Guid userId,
        CreateExpenseRequestDto expenseRequestDto
    )
    {
        var expense = ExpenseMapper.ToEntity(userId, expenseRequestDto);
        var persisted = await _rep.CreateAsync(expense);
        return ExpenseMapper.ToDto(persisted);
    }
}
