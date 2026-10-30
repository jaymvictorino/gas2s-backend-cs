using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Application.Validators;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using Gas2s.Api.Controllers;
using Gas2s.SharedKernel;
using NSubstitute;

namespace Gas2s.UnitTests.Api;

public class ExpensesControllerTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTimeOffset NowUtc = DateTimeOffset.UtcNow;
    private static readonly DateOnly Today = DateOnly.FromDateTime(NowUtc.DateTime);

    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly IExpenseRepository _repo = Substitute.For<IExpenseRepository>();
    private readonly ExpensesController _controller;

    public ExpensesControllerTests()
    {
        _controller = new ExpensesController(
            _user,
            new GetExpensesHandler(_repo),
            new GetExpenseByIdHandler(_repo),
            new CreateExpenseHandler(_repo),
            new UpdateExpenseHandler(_repo),
            new DeleteExpenseHandler(_repo),
            new CreateExpenseRequestDtoValidator(),
            new ExpenseFilterRequestDtoValidator(),
            new UpdateExpenseRequestDtoValidator()
        );

        _user.UserId.Returns(UserId);
    }

    // Expense factory method
    private static Expense NewExpense(
        Guid? userId = null,
        decimal amount = 100m,
        ExpenseCategory category = ExpenseCategory.Groceries,
        string? description = "test",
        DateOnly? date = null,
        TimeOnly? time = null
    ) =>
        Expense.Create(
            userId ?? UserId,
            amount,
            category,
            description,
            date ?? Today,
            time ?? new TimeOnly(12, 0),
            NowUtc
        );

    // CreateExpenseRequestDto factory method
    private static CreateExpenseRequestDto ValidCreateRequest() =>
        new(100m, ExpenseCategory.Groceries, "test", Today, new TimeOnly(12, 0));

    // UpdateExpenseRequestDto factory method
    private static UpdateExpenseRequestDto ValidUpdateRequest() =>
        new(100m, ExpenseCategory.Groceries, "test", Today, new TimeOnly(12, 0));
}
