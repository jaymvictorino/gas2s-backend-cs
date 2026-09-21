using Expenses.Application.Dto;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<ExpenseResponse>> GetAllAsync();
    Task<ExpenseResponse?> GetById(Guid id);
    Task<CreateExpenseRequest> CreateAsync();
}
