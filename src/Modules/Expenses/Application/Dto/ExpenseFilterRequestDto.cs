using Expenses.Application.Enums;

namespace Expenses.Application.Dto;

public record ExpenseFilterRequestDto(ExpenseRangePreset Range, DateOnly? From, DateOnly? To);
