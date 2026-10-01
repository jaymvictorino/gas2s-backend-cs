using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class GetExpenseHandler
{
    private readonly IExpenseRepository _rep;

    public GetExpenseHandler(IExpenseRepository rep) => _rep = rep;

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetAllAsync(Guid userId)
    {
        var retrieved = await _rep.GetAllAsync(userId);
        return [.. retrieved.Select(ExpenseMapper.ToDto)];
    }
}
