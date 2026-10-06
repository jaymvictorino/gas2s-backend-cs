using Expenses.Application.Dto;
using FluentValidation;

namespace Expenses.Application.Validators;

public class UpdateExpenseRequestDtoValidator : AbstractValidator<UpdateExpenseRequestDto>
{
    public UpdateExpenseRequestDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be positive.")
            .Must(a => decimal.Round(a, 2) == a)
            .WithMessage("Amount cannot have more than 2 decimal places.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Invalid expense category.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
    }
}
