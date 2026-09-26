using Expenses.Application.Dto;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetAllAsync(Guid userId);
}
