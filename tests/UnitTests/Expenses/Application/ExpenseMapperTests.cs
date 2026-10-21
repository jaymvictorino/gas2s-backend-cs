using Expenses.Application.Dto;
using Expenses.Application.Mappings;
using Expenses.Domain.Entities;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpenseMapperTests
{
    // ---------- ExpenseMapper.ToEntity ----------

    [Fact]
    public void ToEntity_ConvertCreateExpenseRequestDto_ReturnsExpense()
    {
        var expense = ExpenseMapper.ToEntity(
            ExpensesApplicationFixture.UserId,
            ExpensesApplicationFixture.NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new CreateExpenseRequestDto(amount, category, description, date, time)
            ),
            ExpensesApplicationFixture.NowUtc
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(ExpensesApplicationFixture.UserId);
    }

    // ---------- ExpenseMapper.ToDto ----------

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expResDto = ExpenseMapper.ToDto(
            ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId)
        );

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().NotBeEmpty();
    }
}
