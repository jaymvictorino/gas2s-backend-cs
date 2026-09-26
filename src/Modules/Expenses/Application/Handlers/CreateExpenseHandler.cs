using Expenses.Application.Dto;
using Expenses.Application.Interfaces;

namespace Expenses.Application.Handlers;

public class CreateExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public CreateExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseRequestDto expReq) =>
        await _rep.CreateAsync(expReq);
}
