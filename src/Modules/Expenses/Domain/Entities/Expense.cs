using Expenses.Domain.Enums;

namespace Expenses.Domain.Entities;

public class Expense
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string? Description { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Time { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private Expense() { } // Parameter-less constructor

    public static Expense Create(
        Guid userId,
        decimal amount,
        ExpenseCategory category,
        string description,
        DateOnly date,
        TimeOnly time,
        DateTimeOffset nowUtc
    )
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount must be greater than zero."
            );

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(
                nameof(category),
                "Category must be a valid ExpenseCategory."
            );

        if (description is { Length: > 500 })
            throw new ArgumentException(
                "Description cannot exceed 500 characters.",
                nameof(description)
            );

        return new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Category = category,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Date = date,
            Time = time,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };
    }

    public void Update(
        decimal amount,
        ExpenseCategory category,
        string description,
        DateOnly date,
        TimeOnly time,
        DateTimeOffset nowUtc
    )
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount must be greater than zero."
            );

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(
                nameof(category),
                "Category must be a valid ExpenseCategory."
            );

        if (description is { Length: > 500 })
            throw new ArgumentException(
                "Description cannot exceed 500 characters.",
                nameof(description)
            );

        Amount = amount;
        Category = category;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Date = date;
        Time = time;
        UpdatedAtUtc = nowUtc;
    }
}
