using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Application.Validators;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using Gas2s.Api.Controllers;
using Gas2s.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                    .AddControllers()
                    .Services.BuildServiceProvider(),
            },
        };

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

    // ------------------------------------------------------------------
    // Authentication — every action must reject an unauthenticated request
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("get")]
    [InlineData("getById")]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("delete")]
    public async Task Action_WhenUserNotAuthenticated_ReturnsUnauthorized(string action)
    {
        _user.UserId.Returns((Guid?)null);

        var expenseId = Guid.NewGuid();
        var createReq = ValidCreateRequest();
        var updateReq = ValidUpdateRequest();

        var result = action switch
        {
            "get" => (await _controller.GetExpenses()).Result!,
            "getById" => (await _controller.GetExpenseById(expenseId)).Result!,
            "create" => (await _controller.CreateExpense(createReq)),
            "update" => (await _controller.UpdateExpense(expenseId, updateReq)),
            "delete" => (await _controller.DeleteExpense(expenseId)),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ------------------------------------------------------------------
    // GET /api/expenses
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetExpenses_InvalidFilter_Returns400WithFilterError()
    {
        var result = await _controller.GetExpenses("ewan");

        var error = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;

        error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        error
            .Value.Should()
            .BeOfType<ValidationProblemDetails>()
            .Which.Errors.Keys.Should()
            .Contain("filter");
    }
}
