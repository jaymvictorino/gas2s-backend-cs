using Expenses.Application.Interfaces;
using Expenses.Domain.Entities;
using Expenses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext _expenseDbContext;

    public ExpenseRepository(ExpenseDbContext expenseDbContext) =>
        _expenseDbContext = expenseDbContext;

    public async Task<IReadOnlyList<Expense>> GetAllAsync(Guid userId)
    {
        return await _expenseDbContext
            .Expenses.AsNoTracking()
            .Where(expense => expense.UserId == userId)
            .OrderByDescending(expense => expense.Date)
            .ThenByDescending(expense => expense.Time)
            .ToListAsync();
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
