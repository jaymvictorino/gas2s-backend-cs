using Expenses.Application.Dto;
using Expenses.Application.Enums;
using Expenses.Application.Filtering;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Gas2s.UnitTests.Expenses.Application;

public class ExpensesApplicationHandlersTests
{
    private readonly IExpenseRepository _repo = Substitute.For<IExpenseRepository>();
    private readonly CreateExpenseHandler _create;
    private readonly GetExpensesHandler _get;
    private readonly GetExpenseByIdHandler _getById;
    private readonly UpdateExpenseHandler _update;
    private readonly DeleteExpenseHandler _delete;

    public ExpensesApplicationHandlersTests()
    {
        _create = new CreateExpenseHandler(_repo);
        _get = new GetExpensesHandler(_repo);
        _getById = new GetExpenseByIdHandler(_repo);
        _update = new UpdateExpenseHandler(_repo);
        _delete = new DeleteExpenseHandler(_repo);
    }

    // ---------- CreateExpenseAsync ----------

    [Fact]
    public async Task CreateExpenseAsync_Should_Create_New_Expense()
    {
        var request = ExpensesApplicationFixture.NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _create.CreateExpenseAsync(ExpensesApplicationFixture.UserId, request);

        result.Should().NotBeNull().And.BeOfType<ExpenseResponseDto>();
        await _repo.Received(1).CreateAsync(Arg.Any<Expense>());
    }

    [Fact]
    public async Task CreateExpenseAsync_Returns_Exactly_What_Repository_Returns()
    {
        var request = ExpensesApplicationFixture.NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _create.CreateExpenseAsync(ExpensesApplicationFixture.UserId, request);

        result.Id.Should().NotBeEmpty();
        result.Amount.Should().Be(127.00m);
        result.Category.Should().Be(ExpenseCategory.Clothing);
        result.Description.Should().Be("Gift");
        result.Date.Should().Be(DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime));
        result.Time.Should().Be(TimeOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime));
    }

    // ---------- GetExpensesAsync ----------

    [Fact]
    public async Task GetExpensesAsync_Should_Return_All_Expenses()
    {
        var expenses = new List<Expense>
        {
            ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId),
            ExpensesApplicationFixture.NewExpense(
                ExpensesApplicationFixture.UserId,
                10000m,
                ExpenseCategory.Electronics,
                "Smartphone"
            ),
            ExpensesApplicationFixture.NewExpense(
                Guid.NewGuid(),
                2000,
                ExpenseCategory.Utilities,
                "Electricity"
            ),
        };

        var expected = expenses.FindAll(e => e.UserId == ExpensesApplicationFixture.UserId);

        _repo
            .GetAllAsync(
                ExpensesApplicationFixture.UserId,
                ExpenseDateRangeResolver.Resolve(
                    ExpensesApplicationFixture.Filter,
                    DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime)
                )
            )
            .Returns(expected);

        var result = await _get.GetExpensesAsync(
            ExpensesApplicationFixture.UserId,
            ExpensesApplicationFixture.Filter,
            DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime)
        );

        result.Should().HaveCount(2).And.BeEquivalentTo(expected.Select(ExpenseMapper.ToDto));
    }

    [Fact]
    public async Task GetExpensesAsync_WeekPreset_ReturnsExpensesWithinTheWeek()
    {
        var today = DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime);
        var expectedRange = new ExpenseDateRange(today.AddDays(-6), today);

        var expenses = new List<Expense>()
        {
            ExpensesApplicationFixture.NewExpense(
                ExpensesApplicationFixture.UserId,
                description: "today"
            ),
            ExpensesApplicationFixture.NewExpense(
                ExpensesApplicationFixture.UserId,
                description: "yesterday",
                date: today.AddDays(-1)
            ),
            ExpensesApplicationFixture.NewExpense(
                ExpensesApplicationFixture.UserId,
                description: "last-week",
                date: today.AddDays(-7)
            ),
        };

        var filtered = expenses[..2];
        var expected = filtered.Select(ExpenseMapper.ToDto);

        _repo.GetAllAsync(ExpensesApplicationFixture.UserId, expectedRange).Returns(filtered);

        var result = await _get.GetExpensesAsync(
            ExpensesApplicationFixture.UserId,
            new ExpenseFilterRequestDto(ExpenseRangePreset.Week, null, null),
            today
        );

        result.Should().BeEquivalentTo(expected);
        result.Select(dto => dto.Description).Should().BeEquivalentTo("today", "yesterday");
        await _repo.Received(1).GetAllAsync(ExpensesApplicationFixture.UserId, expectedRange);
    }

    [Fact]
    public async Task GetExpensesAsync_EmptyRepository_Should_Return_EmptyList()
    {
        _repo
            .GetAllAsync(
                ExpensesApplicationFixture.UserId,
                ExpenseDateRangeResolver.Resolve(
                    ExpensesApplicationFixture.Filter,
                    DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime)
                )
            )
            .Returns([]);

        var result = await _get.GetExpensesAsync(
            ExpensesApplicationFixture.UserId,
            ExpensesApplicationFixture.Filter,
            DateOnly.FromDateTime(ExpensesApplicationFixture.NowUtc.DateTime)
        );

        result.Should().BeEmpty();
    }

    // ---------- GetExpenseByIdAsync ----------

    [Fact]
    public async Task GetExpenseByIdAsync_ExistingGuid_Returns_Response()
    {
        var expense = ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId);

        var id = expense.Id;
        var expected = ExpenseMapper.ToDto(expense);

        _repo.GetByIdAsync(ExpensesApplicationFixture.UserId, id).Returns(expense);

        var result = await _getById.GetExpenseByIdAsync(ExpensesApplicationFixture.UserId, id);

        result.Should().Be(expected);
        await _repo.Received(1).GetByIdAsync(ExpensesApplicationFixture.UserId, id);
    }

    [Fact]
    public async Task GetExpenseByIdAsync_MissingGuid_Returns_Null()
    {
        var id = Guid.NewGuid();

        _repo.GetByIdAsync(ExpensesApplicationFixture.UserId, id).Returns((Expense?)null);

        var result = await _getById.GetExpenseByIdAsync(ExpensesApplicationFixture.UserId, id);

        result.Should().BeNull();
    }

    // ---------- UpdateExpenseAsync ----------

    [Fact]
    public async Task UpdateExpenseAsync_NewInformation_ReturnsUpdatedExpense()
    {
        var expense = ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId);

        var dto = ExpensesApplicationFixture.NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new UpdateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.GetByIdAsync(ExpensesApplicationFixture.UserId, expense.Id).Returns(expense);
        _repo.UpdateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _update.UpdateExpenseAsync(
            ExpensesApplicationFixture.UserId,
            expense.Id,
            dto
        );

        result.Should().BeOfType<ExpenseResponseDto>();
        result.Id.Should().Be(expense.Id);
        result.Amount.Should().Be(dto.Amount);
        result.Category.Should().Be(dto.Category);
        result.Description.Should().Be(dto.Description);
        result.Date.Should().Be(dto.Date);
        result.Time.Should().Be(dto.Time);
    }

    [Fact]
    public async Task UpdateExpenseAsync_DifferentUserId_ReturnsNull()
    {
        var dummy = Guid.NewGuid();
        var expenseId = Guid.NewGuid();

        var dto = ExpensesApplicationFixture.NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new UpdateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.GetByIdAsync(dummy, expenseId).Returns((Expense?)null);
        _repo.UpdateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _update.UpdateExpenseAsync(dummy, expenseId, dto);

        result.Should().BeNull();
        await _repo.Received(1).GetByIdAsync(dummy, expenseId);
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Expense>());
    }

    [Fact]
    public async Task UpdateExpenseAsync_DifferentExpenseId_ReturnsNull()
    {
        var dummy = Guid.NewGuid();

        _repo.GetByIdAsync(ExpensesApplicationFixture.UserId, dummy).Returns((Expense?)null);

        var result = await _update.UpdateExpenseAsync(
            ExpensesApplicationFixture.UserId,
            dummy,
            ExpensesApplicationFixture.NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new UpdateExpenseRequestDto(amount, category, description, date, time)
            )
        );

        result.Should().BeNull();
        await _repo.Received(1).GetByIdAsync(ExpensesApplicationFixture.UserId, dummy);
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Expense>());
    }

    // ---------- DeleteExpenseAsync ----------

    [Fact]
    public async Task DeleteExpenseAsync_ExpenseExists_DeletesExpenseReturnsTrue()
    {
        var expense = ExpensesApplicationFixture.NewExpense(ExpensesApplicationFixture.UserId);

        var otherExpense = ExpensesApplicationFixture.NewExpense(
            Guid.NewGuid(),
            2000m,
            ExpenseCategory.Utilities,
            "Electricity"
        );

        var expenses = new List<Expense> { expense, otherExpense };

        _repo.GetByIdAsync(ExpensesApplicationFixture.UserId, expense.Id).Returns(expense);
        _repo
            .DeleteAsync(expense)
            .Returns(true)
            .AndDoes(info => expenses.Remove(info.Arg<Expense>()));

        var result = await _delete.DeleteExpenseAsync(
            ExpensesApplicationFixture.UserId,
            expense.Id
        );

        result.Should().BeTrue();
        expenses.Should().ContainSingle().Which.Amount.Should().Be(2000);
        await _repo.Received(1).GetByIdAsync(ExpensesApplicationFixture.UserId, expense.Id);
        await _repo.Received(1).DeleteAsync(expense);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ExpenseNotFound_ReturnsFalse()
    {
        _repo
            .GetByIdAsync(ExpensesApplicationFixture.UserId, Arg.Any<Guid>())
            .Returns((Expense?)null);

        var result = await _delete.DeleteExpenseAsync(
            ExpensesApplicationFixture.UserId,
            Guid.NewGuid()
        );

        result.Should().BeFalse();
        await _repo.Received(1).GetByIdAsync(ExpensesApplicationFixture.UserId, Arg.Any<Guid>());
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Expense>());
    }
}
