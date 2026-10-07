using Expenses.Application.Interfaces;
using Expenses.Domain.Entities;

namespace Expenses.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    public Task<IReadOnlyList<Expense>> GetAllAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Expense?> GetByIdAsync(Guid userId, Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Expense> CreateAsync(Expense expense)
    {
        throw new NotImplementedException();
    }

    public Task<Expense> UpdateAsync(Expense expense)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Expense expense)
    {
        throw new NotImplementedException();
    }
}
