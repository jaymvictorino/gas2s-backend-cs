using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Application.Filtering;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpenseDateRangeResolverTests
{
    private readonly DateOnly _today = new(2026, 9, 27);

    [Fact]
    public void Resolve_Week_ReturnsExpenseDateRangeWithinLastSevenDays()
    {
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Week, null, null),
            _today
        );

        range.From.Should().Be(new DateOnly(2026, 9, 21));
        range.To.Should().Be(_today);
    }

    [Fact]
    public void Resolve_Week_CrossesMonthBoundary()
    {
        var today = new DateOnly(2026, 9, 1);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Week, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2026, 8, 26));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_Week_CrossesYearBoundary()
    {
        var today = new DateOnly(2026, 1, 1);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Week, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2025, 12, 26));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_Month_ReturnsFirstDayOfTheMonth()
    {
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Month, null, null),
            _today
        );

        range.From.Should().Be(new DateOnly(2026, 9, 1));
        range.To.Should().Be(_today);
    }

    [Fact]
    public void Resolve_Month_OnFirstDayOfMonth_ReturnsSingleDayExpenseDateRange()
    {
        var today = new DateOnly(2026, 9, 1);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Month, null, null),
            today
        );

        range.From.Should().Be(today);
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_Month_OnLastDayOfMonth_ReturnsWholeMonthExpenseDateRange()
    {
        var today = new DateOnly(2026, 9, 30);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Month, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2026, 9, 1));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_Month_LeapYearFebruary_StartsOnFebruaryFirst()
    {
        var today = new DateOnly(2026, 2, 28);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Month, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2026, 2, 1));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_ThreeMonths_ReturnsExpenseDateRangeFromFirstDayOfThreeMonthsBack()
    {
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.ThreeMonths, null, null),
            _today
        );

        range.From.Should().Be(new DateOnly(2026, 7, 1));
        range.To.Should().Be(_today);
    }

    [Fact]
    public void Resolve_ThreeMonths_CrossesYearBoundary()
    {
        var today = new DateOnly(2026, 1, 1);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.ThreeMonths, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2025, 11, 1));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_ThreeMonths_FebruaryGoesBackToDecember()
    {
        var today = new DateOnly(2026, 2, 15);
        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.ThreeMonths, null, null),
            today
        );

        range.From.Should().Be(new DateOnly(2025, 12, 1));
        range.To.Should().Be(today);
    }

    [Fact]
    public void Resolve_Custom_ReturnsExplicitExpenseDateRange()
    {
        var from = new DateOnly(2026, 9, 21);
        var to = new DateOnly(2026, 9, 27);

        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Custom, from, to),
            _today
        );

        range.From.Should().Be(from);
        range.To.Should().Be(to);
    }

    [Fact]
    public void Resolve_Custom_SingleDay_ReturnsSameDay()
    {
        var day = new DateOnly(2026, 9, 21);

        var range = ExpenseDateRangeResolver.Resolve(
            new ExpenseFilterRequestDto(ExpenseRangePreset.Custom, day, day),
            _today
        );

        range.From.Should().Be(day);
        range.To.Should().Be(day);
    }
}
