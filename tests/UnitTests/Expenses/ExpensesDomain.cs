using Expenses.Domain.Enums;
using FluentAssertions;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesDomain
{
    [Fact]
    public void ExpenseCategory_Should_Contain_Expected_Enums()
    {
        var categories = Enum.GetValues<ExpenseCategory>();

        categories
            .Should()
            .BeEquivalentTo([
                ExpenseCategory.Groceries,
                ExpenseCategory.Leisure,
                ExpenseCategory.Electronics,
                ExpenseCategory.Utilities,
                ExpenseCategory.Clothing,
                ExpenseCategory.Health,
                ExpenseCategory.Others,
            ]);
    }
}
