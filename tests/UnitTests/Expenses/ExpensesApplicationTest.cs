using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using NSubstitute;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesApplicationTest
{
    private readonly IExpenseRepository _repo = Substitute.For<IExpenseRepository>();
    private readonly CreateExpenseHandler _create;
    private readonly GetExpenseHandler _get;
    private readonly GetByIdExpenseHandler _getById;

    public ExpensesApplicationTest()
    {
        _create = new CreateExpenseHandler(_repo);
        _get = new GetExpenseHandler(_repo);
        _getById = new GetByIdExpenseHandler(_repo);
    }

    // TODO: Add unit tests
}
