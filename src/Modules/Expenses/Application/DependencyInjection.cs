using Expenses.Application.Handlers;
using Expenses.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddExpensesApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateExpenseHandler>();
        services.AddScoped<GetExpensesHandler>();
        services.AddScoped<GetExpenseByIdHandler>();
        services.AddScoped<UpdateExpenseHandler>();
        services.AddScoped<DeleteExpenseHandler>();

        services.AddValidatorsFromAssemblyContaining<ExpenseFilterRequestDtoValidator>();

        return services;
    }
}
