using Expenses.Domain.Enums;

namespace Expenses.Domain.Entities;

public class Expense
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string? Description { get; private set; }
    public TimeOnly Time { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Expense() { } // Parameter-less constructor

    public static Expense Create(
        Guid userId,
        decimal amount,
        ExpenseCategory category,
        string description,
        DateTime date,
        DateTime nowUtc
    )
    {
        return new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Category = category,
            Date = date,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };
    }

    public void Update(
        decimal amount,
        ExpenseCategory category,
        string description,
        DateTime date,
        DateTime nowUtc
    )
    {
        Amount = amount;
        Category = category;
        Description = description;
        Date = date;
        UpdatedAtUtc = nowUtc;
    }
}
