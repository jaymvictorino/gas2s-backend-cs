using Expenses.Application.Enums;

namespace Gas2s.Api.Common;

internal static class ExpenseRangeParser
{
    public static bool TryParse(string? raw, out ExpenseRangePreset range)
    {
        switch (raw?.Trim().ToLowerInvariant())
        {
            case null or "" or "month":
                range = ExpenseRangePreset.Month;
                return true;

            case "week":
                range = ExpenseRangePreset.Week;
                return true;

            case "3months"
            or "3-months"
            or "threemonths":
                range = ExpenseRangePreset.ThreeMonths;
                return true;

            case "custom":
                range = ExpenseRangePreset.Custom;
                return true;

            default:
                range = default;
                return false;
        }
    }
}
