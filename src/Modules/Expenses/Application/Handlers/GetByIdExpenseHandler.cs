using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class GetByIdExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public GetByIdExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<ExpenseResponseDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var retrieved = await _rep.GetByIdAsync(userId, id);
        return retrieved is not null ? ExpenseMapper.ToDto(retrieved) : null;
    }
}
