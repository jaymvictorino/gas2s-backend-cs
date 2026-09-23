using Expenses.Application.Dto;

namespace Expenses.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<ExpenseResponseDto>> GetAllAsync();
    Task<ExpenseResponseDto?> GetByIdAsync(Guid id);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseRequestDto expReq);
}
