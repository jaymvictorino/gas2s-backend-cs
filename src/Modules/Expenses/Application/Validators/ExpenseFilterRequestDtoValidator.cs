using Expenses.Application.Dto;
using FluentValidation;

namespace Expenses.Application.Validators;

public class ExpenseFilterRequestDtoValidator : AbstractValidator<ExpenseFilterRequestDto>
{
    public ExpenseFilterRequestDtoValidator()
    {
        RuleFor(dto => dto.Range).IsInEnum().WithMessage("Invalid ExpenseRangePreset value.");

        When(
            dto => dto.Range == ExpenseRangePreset.Custom,
            () =>
            {
                RuleFor(dto => dto.From).NotNull().WithMessage("From date required.");
                RuleFor(dto => dto.To).NotNull().WithMessage("To date required.");
            }
        );
    }
}
