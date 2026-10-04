using Expenses.Application.Dto;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;
using Expenses.Application.Validators;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesApplicationTest
{
    private readonly FakeTimeProvider _fakeTime = new(
        new DateTimeOffset(2026, 9, 27, 14, 0, 0, TimeSpan.Zero)
    );

    private readonly Guid _userId = Guid.NewGuid();

    private readonly IExpenseRepository _repo = Substitute.For<IExpenseRepository>();
    private readonly CreateExpenseHandler _create;
    private readonly GetExpensesHandler _get;
    private readonly GetExpenseByIdHandler _getById;
    private readonly UpdateExpenseHandler _update;

    private readonly CreateExpenseRequestDtoValidator _dtoValidator;

    public ExpensesApplicationTest()
    {
        _create = new CreateExpenseHandler(_repo);
        _get = new GetExpensesHandler(_repo);
        _getById = new GetExpenseByIdHandler(_repo);
        _update = new UpdateExpenseHandler(_repo);
        _dtoValidator = new CreateExpenseRequestDtoValidator();
    }

    [Fact]
    public async Task CreateExpenseHandler_Should_Create_New_Expense()
    {
        var request = new CreateExpenseRequestDto(
            127.00m,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _create.CreateExpenseAsync(_userId, request);

        result.Should().NotBeNull().And.BeOfType<ExpenseResponseDto>();
        await _repo.Received(1).CreateAsync(Arg.Any<Expense>());
    }

    [Fact]
    public async Task CreateExpenseHandler_Returns_Exactly_What_Repository_Returns()
    {
        var request = new CreateExpenseRequestDto(
            127.00m,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _create.CreateExpenseAsync(_userId, request);

        result.Id.Should().NotBeEmpty();
        result.Amount.Should().Be(127.00m);
        result.Category.Should().Be(ExpenseCategory.Clothing);
        result.Description.Should().Be("Gift");
        result.Date.Should().Be(DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime));
        result.Time.Should().Be(TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime));
    }

    [Fact]
    public async Task GetExpenseHandler_Should_Return_All_Expenses()
    {
        var expenses = new List<Expense>
        {
            Expense.Create(
                _userId,
                100,
                ExpenseCategory.Groceries,
                "Instant noodles",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                _fakeTime.GetUtcNow()
            ),
            Expense.Create(
                _userId,
                10000,
                ExpenseCategory.Electronics,
                "Smartphone",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                _fakeTime.GetUtcNow()
            ),
            Expense.Create(
                Guid.NewGuid(),
                2000,
                ExpenseCategory.Utilities,
                "Electricity",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                _fakeTime.GetUtcNow()
            ),
        };

        var expected = expenses.FindAll(e => e.UserId == _userId);

        _repo.GetAllAsync(_userId).Returns(expected);

        var result = await _get.GetExpensesAsync(_userId);

        result.Should().HaveCount(2).And.BeEquivalentTo(expected.Select(ExpenseMapper.ToDto));
    }

    [Fact]
    public async Task GetExpenseHandler_EmptyRepository_Should_Return_EmptyList()
    {
        _repo.GetAllAsync(_userId).Returns([]);

        var result = await _get.GetExpensesAsync(_userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdExpenseHandler_ExistingGuid_Returns_Response()
    {
        var expense = Expense.Create(
            _userId,
            100,
            ExpenseCategory.Groceries,
            "Instant noodles",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            _fakeTime.GetUtcNow()
        );

        var id = expense.Id;
        var expected = ExpenseMapper.ToDto(expense);

        _repo.GetByIdAsync(_userId, id).Returns(expense);

        var result = await _getById.GetExpenseByIdAsync(_userId, id);

        result.Should().Be(expected);
        await _repo.Received(1).GetByIdAsync(_userId, id);
    }

    [Fact]
    public async Task GetByIdExpenseHandler_MissingGuid_Returns_Null()
    {
        var id = Guid.NewGuid();

        _repo.GetByIdAsync(_userId, id).Returns((Expense?)null);

        var result = await _getById.GetExpenseByIdAsync(_userId, id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateExpenseAsync_NewInformation_ReturnsUpdatedExpense()
    {
        var expense = Expense.Create(
            _userId,
            100m,
            ExpenseCategory.Groceries,
            "Instant noodles",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            _fakeTime.GetUtcNow()
        );

        _fakeTime.Advance(TimeSpan.FromHours(1));

        var dto = new CreateExpenseRequestDto(
            10000m,
            ExpenseCategory.Electronics,
            "Smartphone",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        _repo.GetByIdAsync(_userId, expense.Id).Returns(expense);
        _repo.UpdateAsync(_userId, Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _update.UpdateExpenseAsync(_userId, expense.Id, dto);

        result.Should().BeOfType<ExpenseResponseDto>();
        result.Id.Should().Be(expense.Id);
        result.Amount.Should().Be(dto.Amount);
        result.Category.Should().Be(dto.Category);
        result.Description.Should().Be(dto.Description);
        result.Date.Should().Be(dto.Date);
        result.Time.Should().Be(dto.Time);
    }

    [Fact]
    public void ToEntity_ConvertCreateExpenseRequestDto_ReturnsExpense()
    {
        var expense = ExpenseMapper.ToEntity(
            _userId,
            new CreateExpenseRequestDto(
                127.00m,
                ExpenseCategory.Clothing,
                "Gift",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
            )
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(_userId);
    }

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expResDto = ExpenseMapper.ToDto(
            Expense.Create(
                _userId,
                127.00m,
                ExpenseCategory.Clothing,
                "Gift",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
                _fakeTime.GetUtcNow()
            )
        );

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void ValidateCreateExpenseRequestDto_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = new CreateExpenseRequestDto(
            amount,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _dtoValidator.Validate(zeroOrNegativeAmount);

        validator.Errors[0].PropertyName.Should().Be("Amount");
        validator.Errors[0].ErrorMessage.Should().Be("Amount must be positive.");
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(12.3456)]
    [InlineData(123.4567)]
    public void ValidateCreateExpenseRequestDto_AmountMoreThanTwoDecimalPlaces_ValidationError(
        decimal amount
    )
    {
        var expenseRequestDto = new CreateExpenseRequestDto(
            amount,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _dtoValidator.Validate(expenseRequestDto);

        validator.Errors[0].PropertyName.Should().Be("Amount");
        validator
            .Errors[0]
            .ErrorMessage.Should()
            .Be("Amount cannot have more than 2 decimal places.");
    }

    [Fact]
    public void ValidateCreateExpenseRequestDto_CategoryNotInExpenseCategory_ValidationError()
    {
        var expenseRequestDto = new CreateExpenseRequestDto(
            127.00m,
            (ExpenseCategory)999,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _dtoValidator.Validate(expenseRequestDto);

        validator.Errors[0].PropertyName.Should().Be("Category");
        validator.Errors[0].ErrorMessage.Should().Be("Invalid expense category.");
    }

    [Fact]
    public void ValidateCreateExpenseRequestDto_DescriptionLengthAbove500_ValidationError()
    {
        var expenseRequestDto = new CreateExpenseRequestDto(
            127.00m,
            ExpenseCategory.Clothing,
            new string('x', 501),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _dtoValidator.Validate(expenseRequestDto);

        validator.Errors[0].PropertyName.Should().Be("Description");
        validator.Errors[0].ErrorMessage.Should().Be("Description cannot exceed 500 characters.");
    }
}
