using Expenses.Application.Dto;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<ExpenseResponse>> GetAllAsync();
    Task<ExpenseResponse?> GetByIdAsync(Guid id);
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequestDto expReq);
}
