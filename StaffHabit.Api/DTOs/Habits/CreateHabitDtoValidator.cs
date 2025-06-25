using FluentValidation;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits = ["minutes", "hours", "steps", "km", "cal", "pages", "tasks", "sessions"];
    private static readonly string[] AllowedUnitsForbinaryHabits = ["sessions", "tasks"];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .WithMessage("habit name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid habit type");

        RuleFor(x => x.Frequency.Type)
            .IsInEnum()
            .WithMessage("Invalid frequency period");

        RuleFor(x => x.Frequency.TimesPerPeriod)
            .GreaterThan(0)
            .WithMessage("Frequency musyt be greater than 0");

        RuleFor(x => x.Target.Value)
            .GreaterThan(0)
            .WithMessage("Target must be greater than 0");

        RuleFor(x => x.Target.Unit)
            .NotEmpty()
            .Must(unit => AllowedUnits.Contains(unit.ToLowerInvariant()))
            .WithMessage($"Unit must be one of: {string.Join(", ", AllowedUnits)}");

        RuleFor(x => x.EndDate)
            .Must(date => date is null || date.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in the future");

        When(x => x.Milestone is not null, () =>
        {
            RuleFor(x => x.Milestone!.Target)
            .GreaterThan(0).WithMessage("Milestone target must be greater than 0");
        });

        RuleFor(x => x.Target.Unit)
            .Must((dto, unit) => IsTargetUnitCompatibleWithType(dto.Type, unit))
            .WithMessage("Target unit is not caompatible with the habit type");
    }

    private static bool IsTargetUnitCompatibleWithType(HabitType type, string unit)
    {
        string normalizedUnit = unit.ToLowerInvariant();
        return type switch
        {
            HabitType.Binary => AllowedUnitsForbinaryHabits.Contains(normalizedUnit),
            HabitType.Measurable => AllowedUnits.Contains(normalizedUnit),
            _ => false,
        };
    }
}
