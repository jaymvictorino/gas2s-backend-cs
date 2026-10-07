using Expenses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Infrastructure.Data;

public class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
        : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();
}
