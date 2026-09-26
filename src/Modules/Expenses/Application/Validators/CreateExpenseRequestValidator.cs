using Expenses.Application.Dto;
using FluentValidation;

namespace Expenses.Application.Validators;

public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequestDto>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Invalid expense category");
    }
}
