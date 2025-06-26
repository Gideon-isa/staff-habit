using FluentValidation;

namespace StaffHabit.Api.DTOs.Tags;

public sealed class  CreateTagValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .MinimumLength(3)
            .WithMessage("Tag name must be at least 3 characters.");

        RuleFor(t => t.Description)
            .MaximumLength(50)
            .WithMessage("Tag name must not exceed 100 characters.");
    }
}
