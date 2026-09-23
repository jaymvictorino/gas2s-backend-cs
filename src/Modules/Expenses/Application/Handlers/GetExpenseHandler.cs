using Expenses.Application.Dto;
using Expenses.Application.Interfaces;

namespace Expenses.Application.Handlers;

public class GetExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public GetExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<IReadOnlyList<ExpenseResponse>> GetAllAsync() => await _rep.GetAllAsync();
}
