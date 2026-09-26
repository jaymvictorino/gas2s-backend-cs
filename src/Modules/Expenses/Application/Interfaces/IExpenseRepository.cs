using Expenses.Domain.Entities;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetAllAsync(Guid userId);
    Task<Expense?> GetByIdAsync(Guid userId, Guid);
}
