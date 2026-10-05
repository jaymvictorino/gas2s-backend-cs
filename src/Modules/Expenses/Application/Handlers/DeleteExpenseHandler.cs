using Expenses.Application.Interfaces;

namespace Expenses.Application.Handlers;

public class DeleteExpenseHandler
{
    private readonly IExpenseRepository _repo;

    public DeleteExpenseHandler(IExpenseRepository repo) => _repo = repo;

    public async Task<bool?> DeleteExpenseAsync(Guid userId, Guid id)
    {
        var retrieved = await _repo.GetByIdAsync(userId, id);

        if (retrieved is null)
            return null;

        return await _repo.DeleteAsync(retrieved);
    }
}
