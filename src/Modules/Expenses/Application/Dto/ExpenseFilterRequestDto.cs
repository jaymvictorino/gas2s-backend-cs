namespace Expenses.Application.Dto;

public record ExpenseFilterRequestDto(ExpenseRangePreset Range, DateOnly? From, DateOnly? To);

public enum ExpenseRangePreset
{
    Week,
    Month,
    ThreeMonths,
    Custom,
}
