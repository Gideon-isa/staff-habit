namespace StaffHabit.Api.DTOs.HabitTag;

public sealed record UpsertHabitTagsDto
{
    public required List<string> TagIds { get; init; }
}
