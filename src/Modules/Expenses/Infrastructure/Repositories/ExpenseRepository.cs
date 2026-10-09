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

    public async Task<Expense?> GetByIdAsync(Guid userId, Guid id)
    {
        return await _expenseDbContext.Expenses.SingleOrDefaultAsync(expense =>
            expense.UserId == userId && expense.Id == id
        );
    }

    public async Task<Expense> CreateAsync(Expense expense)
    {
        _expenseDbContext.Expenses.Add(expense);
        await _expenseDbContext.SaveChangesAsync();
        return expense;
    }

    public async Task<Expense> UpdateAsync(Expense expense)
    {
        await _expenseDbContext.SaveChangesAsync();
        return expense;
    }

    public Task<bool> DeleteAsync(Expense expense)
    {
        throw new NotImplementedException();
    }
}
