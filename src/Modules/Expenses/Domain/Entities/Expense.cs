using Expenses.Domain.Enums;

namespace Expenses.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime UpdatedAt { get; set; }
}
