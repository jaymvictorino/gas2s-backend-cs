using Expenses.Application.Interfaces;
using Expenses.Infrastructure.Data;
using Expenses.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExpensesInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<ExpenseDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        return services;
    }
}
