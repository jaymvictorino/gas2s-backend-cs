using Expenses.Application.Dto;
using Expenses.Application.Filtering;
using Expenses.Application.Handlers;
using Expenses.Application.Interfaces;
using Expenses.Application.Mappings;
using Expenses.Application.Validators;
using Expenses.Domain.Entities;
using Expenses.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Gas2s.UnitTests.Expenses;

public class ExpensesApplicationTest
{
    private readonly FakeTimeProvider _fakeTime = new(
        new DateTimeOffset(2026, 9, 27, 14, 0, 0, TimeSpan.Zero)
    );

    private readonly DateTimeOffset _nowUtc = new(2026, 9, 27, 14, 0, 0, TimeSpan.Zero);

    private readonly Guid _userId = Guid.NewGuid();

    private readonly ExpenseFilterRequestDto _filter = new(ExpenseRangePreset.Week, null, null);

    private readonly IExpenseRepository _repo = Substitute.For<IExpenseRepository>();
    private readonly CreateExpenseHandler _create;
    private readonly GetExpensesHandler _get;
    private readonly GetExpenseByIdHandler _getById;
    private readonly UpdateExpenseHandler _update;
    private readonly DeleteExpenseHandler _delete;

    private readonly CreateExpenseRequestDtoValidator _createExpenseRequestDtoValidator;
    private readonly UpdateExpenseRequestDtoValidator _updateExpenseRequestDtoValidator;

    public ExpensesApplicationTest()
    {
        _create = new CreateExpenseHandler(_repo);
        _get = new GetExpensesHandler(_repo);
        _getById = new GetExpenseByIdHandler(_repo);
        _update = new UpdateExpenseHandler(_repo);
        _delete = new DeleteExpenseHandler(_repo);
        _createExpenseRequestDtoValidator = new CreateExpenseRequestDtoValidator();
        _updateExpenseRequestDtoValidator = new UpdateExpenseRequestDtoValidator();
    }

    private Expense NewExpense(
        Guid userId,
        decimal amount = 100m,
        ExpenseCategory category = ExpenseCategory.Groceries,
        string? description = "test",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return Expense.Create(
            userId,
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(_nowUtc.DateTime),
            time ?? TimeOnly.FromDateTime(_nowUtc.DateTime),
            _nowUtc
        );
    }

    private T NewExpenseRequestDto<T>(
        Func<decimal, ExpenseCategory, string, DateOnly, TimeOnly, T> factory,
        decimal amount = 127.00m,
        ExpenseCategory category = ExpenseCategory.Clothing,
        string description = "Gift",
        DateOnly? date = null,
        TimeOnly? time = null
    )
    {
        return factory(
            amount,
            category,
            description,
            date ?? DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            time ?? TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );
    }

    // ---------- CreateExpenseAsync ----------

    [Fact]
    public async Task CreateExpenseAsync_Should_Create_New_Expense()
    {
        var request = NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.CreateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

        var result = await _create.CreateExpenseAsync(_userId, request);

        result.Should().NotBeNull().And.BeOfType<ExpenseResponseDto>();
        await _repo.Received(1).CreateAsync(Arg.Any<Expense>());
    }

    [Fact]
    public async Task CreateExpenseHandler_Returns_Exactly_What_Repository_Returns()
    {
        var request = NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
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

    // ---------- GetExpensesAsync ----------

    [Fact]
    public async Task GetExpensesAsync_Should_Return_All_Expenses()
    {
        var expenses = new List<Expense>
        {
            NewExpense(_userId),
            NewExpense(_userId, 10000m, ExpenseCategory.Electronics, "Smartphone"),
            NewExpense(Guid.NewGuid(), 2000, ExpenseCategory.Utilities, "Electricity"),
        };

        var expected = expenses.FindAll(e => e.UserId == _userId);

        _repo
            .GetAllAsync(
                _userId,
                ExpenseDateRangeResolver.Resolve(_filter, DateOnly.FromDateTime(_nowUtc.DateTime))
            )
            .Returns(expected);

        var result = await _get.GetExpensesAsync(
            _userId,
            _filter,
            DateOnly.FromDateTime(_nowUtc.DateTime)
        );

        result.Should().HaveCount(2).And.BeEquivalentTo(expected.Select(ExpenseMapper.ToDto));
    }

    [Fact]
    public async Task GetExpensesAsync_EmptyRepository_Should_Return_EmptyList()
    {
        _repo
            .GetAllAsync(
                _userId,
                ExpenseDateRangeResolver.Resolve(
                    _filter,
                    DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
                )
            )
            .Returns([]);

        var result = await _get.GetExpensesAsync(
            _userId,
            _filter,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        result.Should().BeEmpty();
    }

    // ---------- GetExpenseByIdAsync ----------

    [Fact]
    public async Task GetExpenseByIdAsync_ExistingGuid_Returns_Response()
    {
        var expense = NewExpense(_userId);

        var id = expense.Id;
        var expected = ExpenseMapper.ToDto(expense);

        _repo.GetByIdAsync(_userId, id).Returns(expense);

        var result = await _getById.GetExpenseByIdAsync(_userId, id);

        result.Should().Be(expected);
        await _repo.Received(1).GetByIdAsync(_userId, id);
    }

    [Fact]
    public async Task GetExpenseByIdAsync_MissingGuid_Returns_Null()
    {
        var id = Guid.NewGuid();

        _repo.GetByIdAsync(_userId, id).Returns((Expense?)null);

        var result = await _getById.GetExpenseByIdAsync(_userId, id);

        result.Should().BeNull();
    }

    // ---------- UpdateExpenseAsync ----------

    [Fact]
    public async Task UpdateExpenseAsync_NewInformation_ReturnsUpdatedExpense()
    {
        var expense = NewExpense(_userId);

        var dto = NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new UpdateExpenseRequestDto(amount, category, description, date, time)
        );

        _repo.GetByIdAsync(_userId, expense.Id).Returns(expense);
        _repo.UpdateAsync(Arg.Any<Expense>()).Returns(info => info.Arg<Expense>());

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
    public async Task UpdateExpenseAsync_DifferentUserId_ReturnsNull()
    {
        var dummy = Guid.NewGuid();
        var expenseId = Guid.NewGuid();

        var dto = NewExpenseRequestDto(
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

        _repo.GetByIdAsync(_userId, dummy).Returns((Expense?)null);

        var result = await _update.UpdateExpenseAsync(
            _userId,
            dummy,
            NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new UpdateExpenseRequestDto(amount, category, description, date, time)
            )
        );

        result.Should().BeNull();
        await _repo.Received(1).GetByIdAsync(_userId, dummy);
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Expense>());
    }

    // ---------- DeleteExpenseAsync ----------

    [Fact]
    public async Task DeleteExpenseAsync_ExpenseExists_DeletesExpenseReturnsTrue()
    {
        var expense = NewExpense(_userId);

        var otherExpense = NewExpense(
            Guid.NewGuid(),
            2000m,
            ExpenseCategory.Utilities,
            "Electricity"
        );

        var expenses = new List<Expense> { expense, otherExpense };

        _repo.GetByIdAsync(_userId, expense.Id).Returns(expense);
        _repo
            .DeleteAsync(expense)
            .Returns(true)
            .AndDoes(info => expenses.Remove(info.Arg<Expense>()));

        var result = await _delete.DeleteExpenseAsync(_userId, expense.Id);

        result.Should().BeTrue();
        expenses.Should().ContainSingle().Which.Amount.Should().Be(2000);
        await _repo.Received(1).GetByIdAsync(_userId, expense.Id);
        await _repo.Received(1).DeleteAsync(expense);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ExpenseNotFound_ReturnsFalse()
    {
        _repo.GetByIdAsync(_userId, Arg.Any<Guid>()).Returns((Expense?)null);

        var result = await _delete.DeleteExpenseAsync(_userId, Guid.NewGuid());

        result.Should().BeFalse();
        await _repo.Received(1).GetByIdAsync(_userId, Arg.Any<Guid>());
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Expense>());
    }

    // ---------- ExpenseMapper.ToEntity ----------

    [Fact]
    public void ToEntity_ConvertCreateExpenseRequestDto_ReturnsExpense()
    {
        var expense = ExpenseMapper.ToEntity(
            _userId,
            NewExpenseRequestDto(
                (amount, category, description, date, time) =>
                    new CreateExpenseRequestDto(amount, category, description, date, time)
            )
        );

        expense.Should().BeOfType<Expense>();
        expense.UserId.Should().Be(_userId);
    }

    // ---------- ExpenseMapper.ToDto ----------

    [Fact]
    public void ToDto_ConvertExpense_ReturnsExpenseResponseDto()
    {
        var expResDto = ExpenseMapper.ToDto(NewExpense(_userId));

        expResDto.Should().BeOfType<ExpenseResponseDto>();
        expResDto.Id.Should().NotBeEmpty();
    }

    // ---------- CreateExpenseRequestDtoValidator ----------

    [Theory]
    [InlineData(0m)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void CreateExpenseRequestDtoValidator_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(zeroOrNegativeAmount);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount must be positive.");
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(12.3456)]
    [InlineData(123.4567)]
    public void CreateExpenseRequestDtoValidator_AmountMoreThanTwoDecimalPlaces_ValidationError(
        decimal amount
    )
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (_, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount cannot have more than 2 decimal places.");
    }

    [Fact]
    public void CreateExpenseRequestDtoValidator_CategoryNotInExpenseCategory_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Category)
            .WithErrorMessage("Invalid expense category.");
    }

    [Fact]
    public void CreateExpenseRequestDtoValidator_DescriptionLengthAbove500_ValidationError()
    {
        var expenseRequestDto = NewExpenseRequestDto(
            (amount, category, description, date, time) =>
                new CreateExpenseRequestDto(amount, category, description, date, time)
        );

        var validator = _createExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void UpdateExpenseRequestDtoValidator_AmountLessThanOrEqualToZero_ValidationError(
        decimal amount
    )
    {
        var zeroOrNegativeAmount = new UpdateExpenseRequestDto(
            amount,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(zeroOrNegativeAmount);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount must be positive.");
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(12.3456)]
    [InlineData(123.4567)]
    public void UpdateExpenseRequestDtoValidator_AmountMoreThanTwoDecimalPlaces_ValidationError(
        decimal amount
    )
    {
        var expenseRequestDto = new UpdateExpenseRequestDto(
            amount,
            ExpenseCategory.Clothing,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Amount)
            .WithErrorMessage("Amount cannot have more than 2 decimal places.");
    }

    [Fact]
    public void UpdateExpenseRequestDtoValidator_CategoryNotInExpenseCategory_ValidationError()
    {
        var expenseRequestDto = new UpdateExpenseRequestDto(
            127.00m,
            (ExpenseCategory)999,
            "Gift",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Category)
            .WithErrorMessage("Invalid expense category.");
    }

    [Fact]
    public void UpdateExpenseRequestDtoValidator_DescriptionLengthAbove500_ValidationError()
    {
        var expenseRequestDto = new UpdateExpenseRequestDto(
            127.00m,
            ExpenseCategory.Clothing,
            new string('x', 501),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime),
            TimeOnly.FromDateTime(_fakeTime.GetUtcNow().DateTime)
        );

        var validator = _updateExpenseRequestDtoValidator.TestValidate(expenseRequestDto);

        validator
            .ShouldHaveValidationErrorFor(dto => dto.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }
}
