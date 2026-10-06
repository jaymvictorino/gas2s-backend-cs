using Expenses.Application.Dto;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;

namespace Expenses.Application.Handlers;

public class GetExpensesHandler
{
    private readonly IExpenseRepository _repo;

    public GetExpensesHandler(IExpenseRepository rep) => _repo = rep;

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(Guid userId)
    {
        var retrieved = await _repo.GetAllAsync(userId);
        return [.. retrieved.Select(ExpenseMapper.ToDto)];
    }
}
