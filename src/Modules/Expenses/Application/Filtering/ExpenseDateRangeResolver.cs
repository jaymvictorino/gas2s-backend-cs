using Expenses.Application.Dto;

namespace Expenses.Application.Filtering;

public static class ExpenseDateRangeResolver
{
    public static ExpenseDateRange Resolve(ExpenseFilterRequestDto filter, DateOnly today) =>
        filter.Range switch
        {
            ExpenseRangePreset.Week => new ExpenseDateRange(today.AddDays(-6), today),
            ExpenseRangePreset.Month => new ExpenseDateRange(
                new DateOnly(today.Year, today.Month, 1),
                today
            ),
            ExpenseRangePreset.ThreeMonths => new ExpenseDateRange(
                new DateOnly(today.Year, today.Month, 1).AddMonths(-2),
                today
            ),
            ExpenseRangePreset.Custom => new ExpenseDateRange(filter.From!.Value, filter.To!.Value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(filter),
                filter.Range,
                "Invalid ExpenseRangePreset value."
            ),
        };
}
