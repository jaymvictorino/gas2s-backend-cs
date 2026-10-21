using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesApplicationFixture
{
    protected internal DateTimeOffset NowUtc = new(2026, 9, 27, 14, 0, 0, TimeSpan.Zero);

    protected internal Guid UserId = Guid.NewGuid();

    protected internal ExpenseFilterRequestDto Filter = new(ExpenseRangePreset.Week, null, null);

    protected internal Expense NewExpense(
        Guid userId,
        decimal amount = 100m,
        ExpenseCategory category = ExpenseCategory.Groceries,
        string? description = "test",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return Expense.Create(
            userId,
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(NowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(NowUtc.DateTime),
            NowUtc
        );
    }

    protected internal T NewExpenseRequestDto<T>(
        Func<decimal, ExpenseCategory, string, DateOnly, TimeOnly, T> factory,
        decimal amount = 127.00m,
        ExpenseCategory category = ExpenseCategory.Clothing,
        string description = "Gift",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return factory(
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(NowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(NowUtc.DateTime)
        );
    }
}
