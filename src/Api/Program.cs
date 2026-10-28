using Expenses.Application;
using Expenses.Infrastructure;
using Gas2s.Api.Common;
using Gas2s.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Current user
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, DevCurrentUser>();

// Expenses modules
builder.Services.AddExpensesApplication();
builder.Services.AddExpensesInfrastructure(
    builder.Configuration.GetConnectionString("Gas2sDb")
        ?? throw new InvalidOperationException("ConnectionStrings:Gas2sDb not configured.")
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
