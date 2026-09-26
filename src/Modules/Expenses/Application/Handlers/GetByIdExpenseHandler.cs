using Expenses.Application.Dto;
using Expenses.Application.Interfaces;

namespace Expenses.Application.Handlers;

public class GetByIdExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public GetByIdExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<ExpenseResponseDto?> GetByIdAsync(Guid id) => await _rep.GetByIdAsync(id);
}
