using Expenses.Application.Filtering;
using Expenses.Domain.Entities;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetAllAsync(Guid userId, ExpenseDateRange dateRange);
    Task<Expense?> GetByIdAsync(Guid userId, Guid id);
    Task<Expense> CreateAsync(Expense expense);
    Task<Expense> UpdateAsync(Expense expense);
    Task<bool> DeleteAsync(Expense expense);
}
