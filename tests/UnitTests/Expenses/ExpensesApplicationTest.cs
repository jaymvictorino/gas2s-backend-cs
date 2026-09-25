using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Domain.Enums;
using FluentAssertions;
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

    [Fact]
    public async Task CreateExpenseHandler_Should_Create_New_Expense()
    {
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateTime(2026, 9, 22, 2, 11, 0, DateTimeKind.Utc);

        var request = new CreateExpenseRequestDto(amt, cat, desc, date);
        var response = new ExpenseResponseDto(Guid.NewGuid(), amt, cat, desc, date);

        _repo.CreateAsync(request).Returns(response);

        var result = await _create.CreateExpenseAsync(request);

        result.Should().Be(response);
        await _repo.Received(1).CreateAsync(request);
    }

    [Fact]
    public async Task CreateExpenseHandler_Returns_Exactly_What_Repository_Returns()
    {
        const decimal amt = 127.00m;
        const ExpenseCategory cat = ExpenseCategory.Clothing;
        const string desc = "Gift for Liji";
        var date = new DateTime(2026, 9, 22, 2, 11, 0, DateTimeKind.Utc);

        var request = new CreateExpenseRequestDto(amt, cat, desc, date);
        var response = new ExpenseResponseDto(Guid.NewGuid(), amt, cat, desc, date);

        _repo.CreateAsync(request).Returns(response);

        var result = await _create.CreateExpenseAsync(request);

        result.Id.Should().Be(response.Id);
        result.Amount.Should().Be(response.Amount);
        result.Category.Should().Be(response.Category);
        result.Description.Should().Be(response.Description);
        result.Date.Should().Be(response.Date);
    }

    [Fact]
    public async Task GetExpenseHandler_Should_Return_All_Expenses()
    {
        var expenses = new List<ExpenseResponseDto>
        {
            new(Guid.NewGuid(), 100, ExpenseCategory.Groceries, "Instant noodles", DateTime.Now),
            new(
                Guid.NewGuid(),
                10000,
                ExpenseCategory.Electronics,
                "Smartphone",
                new DateTime(2026, 9, 22, 2, 11, 0, DateTimeKind.Utc)
            ),
        };

        _repo.GetAllAsync().Returns(expenses);

        var result = await _get.GetAllAsync();

        result.Should().HaveCount(2).And.BeEquivalentTo(expenses);
    }

    [Fact]
    public async Task GetExpenseHandler_EmptyRepository_Should_Return_EmptyList()
    {
        var expenses = new List<ExpenseResponseDto>();

        _repo.GetAllAsync().Returns(expenses);

        var result = await _get.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdExpenseHandler_ExistingGuid_Returns_Response()
    {
        var guid = Guid.NewGuid();
        var expense = new ExpenseResponseDto(
            guid,
            100,
            ExpenseCategory.Groceries,
            "Instant noodles",
            DateTime.Now
        );

        _repo.GetByIdAsync(guid).Returns(expense);

        var result = await _getById.GetByIdAsync(guid);

        result.Should().Be(expense);
        await _repo.Received(1).GetByIdAsync(guid);
    }

    [Fact]
    public async Task GetByIdExpenseHandler_MissingGuid_Returns_Null()
    {
        var guid = Guid.NewGuid();

        _repo.GetByIdAsync(guid).Returns((ExpenseResponseDto?)null);

        var result = await _getById.GetByIdAsync(guid);

        result.Should().BeNull();
    }
}
