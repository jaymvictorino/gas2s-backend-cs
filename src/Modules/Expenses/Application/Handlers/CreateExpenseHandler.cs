using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class CreateExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public CreateExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<ExpenseResponseDto> CreateExpenseAsync(
        CreateExpenseRequestDto expenseRequestDto,
        Guid userId
    )
    {
        var expense = ExpenseMapper.ToEntity(expenseRequestDto, userId);
        var persisted = await _rep.CreateAsync(expense);
        return ExpenseMapper.ToDto(persisted);
    }
}
