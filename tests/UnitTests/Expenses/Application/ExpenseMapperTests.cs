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
        var dto = ExpensesApplicationFixture.NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );
        var expense = ExpenseMapper.ToEntity(
            ExpensesApplicationFixture.UserId,
            dto,
            ExpensesApplicationFixture.NowUtc
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(ExpensesApplicationFixture.UserId);
        expense.Amount.Should().Be(dto.Amount);
        expense.Category.Should().Be(dto.Category);
        expense.Description.Should().Be(dto.Description);
        expense.Date.Should().Be(dto.Date);
        expense.Time.Should().Be(dto.Time);
    }

    // ---------- ExpenseMapper.ToDto ----------

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expense = ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId);

        var expResDto = ExpenseMapper.ToDto(expense);

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().Be(expense.Id);
        expResDto.Amount.Should().Be(expense.Amount);
        expResDto.Category.Should().Be(expense.Category);
        expResDto.Description.Should().Be(expense.Description);
        expResDto.Date.Should().Be(expense.Date);
        expResDto.Time.Should().Be(expense.Time);
    }
}
