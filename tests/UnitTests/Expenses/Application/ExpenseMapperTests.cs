using Expenses.Application.Dto;
using Expenses.Application.Mappings;
using Expenses.Domain.Entities;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpenseMapperTests
{
    private readonly ExpensesApplicationTest _application;

    public ExpenseMapperTests() => _application = new ExpensesApplicationTest();

    // ---------- ExpenseMapper.ToEntity ----------

    [Fact]
    public void ToEntity_ConvertCreateExpenseRequestDto_ReturnsExpense()
    {
        var expense = ExpenseMapper.ToEntity(
            _application.UserId,
            _application.NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new CreateExpenseRequestDto(amount, category, description, date, time)
            ),
            _application.NowUtc
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(_application.UserId);
    }

    // ---------- ExpenseMapper.ToDto ----------

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expResDto = ExpenseMapper.ToDto(_application.NewExpense(_application.UserId));

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().NotBeEmpty();
    }
}
