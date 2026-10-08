using Expenses.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(expense => expense.Id);

        builder.Property(expense => expense.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(expense => expense.UserId).HasColumnName("user_id").IsRequired();

        builder
            .Property(expense => expense.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(expense => expense.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .IsRequired();

        builder
            .Property(expense => expense.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(expense => expense.Date).HasColumnName("date").IsRequired();

        builder.Property(expense => expense.Time).HasColumnName("time").IsRequired();

        builder
            .Property(expense => expense.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(expense => expense.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.HasIndex(expense => expense.UserId).HasDatabaseName("ix_expenses_user_id");
    }
}
