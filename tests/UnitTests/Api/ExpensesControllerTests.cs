using Expenses.Application.Dto;
using Expenses.Application.Filtering;
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

    [Fact]
    public async Task GetExpenses_ValidRequest_Returns200WithExpenses()
    {
        var expenses = new List<Expense>
        {
            NewExpense(description: "a"),
            NewExpense(description: "b"),
        };

        _repo.GetAllAsync(UserId, Arg.Any<ExpenseDateRange>()).Returns(expenses);

        var result = await _controller.GetExpenses("week");

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ok.Value.Should().BeAssignableTo<IReadOnlyList<ExpenseResponseDto>>().Subject;
        items.Should().HaveCount(2);
        items.Select(dto => dto.Description).Should().BeEquivalentTo("a", "b");
    }

    // ------------------------------------------------------------------
    // GET /api/expenses/{id}
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetExpenseById_Found_Returns200OK()
    {
        var expense = NewExpense(description: "found");

        _repo.GetByIdAsync(UserId, expense.Id).Returns(expense);

        var result = await _controller.GetExpenseById(expense.Id);

        result
            .Result.Should()
            .BeOfType<OkObjectResult>()
            .Which.Value.Should()
            .BeOfType<ExpenseResponseDto>()
            .Which.Id.Should()
            .Be(expense.Id);
        await _repo.Received(1).GetByIdAsync(UserId, expense.Id);
    }

    [Fact]
    public async Task GetExpensesById_NotFound_Returns404NotFound()
    {
        _repo.GetByIdAsync(UserId, Arg.Any<Guid>()).Returns((Expense?)null);

        var result = await _controller.GetExpenseById(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundResult>();
        await _repo.Received(1).GetByIdAsync(UserId, Arg.Any<Guid>());
    }

    // ------------------------------------------------------------------
    // POST /api/expenses
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateExpense_Valid_Returns201WithLocationOfNewExpenseId()
    {
        var createReq = ValidCreateRequest();

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _controller.CreateExpense(createReq);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ExpensesController.GetExpenseById));

        var body = created.Value.Should().BeAssignableTo<ExpenseResponseDto>().Subject;

        created.RouteValues!["id"].Should().Be(body.Id);
    }

    [Fact]
    public async Task CreateExpense_InvalidAmount_Returns400WithAmountError()
    {
        var badReq = ValidCreateRequest() with { Amount = -1m };

        var result = await _controller.CreateExpense(badReq);

        var problem = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        problem
            .Value.Should()
            .BeOfType<ValidationProblemDetails>()
            .Which.Errors.Keys.Should()
            .Contain("Amount");
    }
}
