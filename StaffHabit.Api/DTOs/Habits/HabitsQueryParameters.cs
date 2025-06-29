using Microsoft.AspNetCore.Mvc;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.DTOs.Habits;

public sealed class HabitsQueryParameters
{
    [FromQuery(Name ="q")]
    public string? Search { get; set; }
    public HabitType? Type { get; set; }
    public HabitStatus? Status { get; set; }
    public string? Sort { get; set; }
}
