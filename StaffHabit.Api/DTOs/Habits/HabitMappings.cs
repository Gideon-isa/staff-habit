using System.Security.Cryptography.X509Certificates;
using StaffHabit.Api.Entities;
using StaffHabit.Api.Services.Sorting;

namespace StaffHabit.Api.DTOs.Habits;

internal static class HabitMappings
{
    public static readonly SortMappingDefinition<HabitDto, Habit> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(HabitDto.Name), nameof(Habit.Name)),
            new SortMapping(nameof(HabitDto.Description), nameof(Habit.Description)),
            new SortMapping(nameof(HabitDto.Type), nameof(Habit.Type)),
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(HabitDto.Type)}", $"{nameof(Habit.Frequency)}.{nameof(Habit.Type)}"),
            new SortMapping($"{nameof(HabitDto.Frequency)}.{nameof(FrequencyDto.TimesPerPeriod)}", $"{nameof(Habit.Frequency)}.{nameof(Frequency.TimesPeriod)}"),
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Value)}", $"{nameof(Habit.Target)}.{nameof(Target.Value)}"),
            new SortMapping($"{nameof(HabitDto.Target)}.{nameof(TargetDto.Unit)}", $"{nameof(Habit.Target)}.{nameof(Target.Unit)}"),
            new SortMapping(nameof(HabitDto.Status), nameof(Habit.Status)),
            new SortMapping(nameof(HabitDto.EndDate), nameof(Habit.EndDate)),
            new SortMapping(nameof(HabitDto.CreatedAtUtc), nameof(Habit.CreatedAtUtc)),
            new SortMapping(nameof(HabitDto.UpdatedAtUtc), nameof(Habit.UpdatedAtUtc)),
            new SortMapping(nameof(HabitDto.LastCompletedAtUtc), nameof(Habit.LastCompletedAtUtc)),
        ]
    };
    public static Habit ToEntity(this CreateHabitDto dto)
    {

        Habit habit = new()
        {
            Id = $"h_{Guid.NewGuid().ToString()}",
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Frequency = new Frequency
            {
                Type = dto.Frequency.Type,
                TimesPeriod = dto.Frequency.TimesPerPeriod,
            },
            Target = new Target
            {
                Value = dto.Target.Value,
                Unit = dto.Target.Unit
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            EndDate = dto.EndDate,
            Milestone = dto.Milestone == null ? null : new Milestone
            {
                Target = dto.Milestone.Target,
                Current = dto.Milestone.Current
            },
            CreatedAtUtc = DateTime.UtcNow
        };
        return habit;
    }

    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto()
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPeriod
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived, 
            EndDate = habit.EndDate,
            Milestone = habit.Milestone == null ? null : new MilestoneDto
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current
            },
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }

    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        
        // Update basic properties
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;

        // Update frequency (assuming it's immutable, create new instance)
        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimesPeriod = dto.Frequency.TimesPerPeriod
        };
        // Update target
        habit.Target = new Target
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };
        //Update milestone if provided
        if (dto.Milestone is not null)
        {
            habit.Milestone ??= new Milestone();
            habit.Milestone.Target = dto.Milestone.Target;
            // Note: We don't update Milestone.Current from DTO to preserve progress
        }
        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
}
