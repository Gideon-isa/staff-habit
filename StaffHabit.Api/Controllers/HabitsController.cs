using System.Linq.Expressions;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Database;
using StaffHabit.Api.DTOs.Habits;
using StaffHabit.Api.Entities;
using StaffHabit.Api.Services.Sorting;
using StaffHabit.Api.Services.Utility;

namespace StaffHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits([FromQuery] HabitsQueryParameters query, SortMappingProvider sortMappingProvider)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Sort Field"
            );
        }
        query.Search ??= query.Search?.Trim().ToLower();
        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        List<HabitDto> habits = await dbContext
            .Habits
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .ApplySort(query.Sort, sortMappings)
            .Where(h => query.Status == null || h.Status == query.Status)
            .Select(HabitQueries.ProjectToDto()).ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Data = habits
        };
        return Ok(habitsCollectionDto);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit([FromRoute] string id)
    {
        HabitWithTagsDto habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto()).FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound($"Habit with ID {id} not found.");
        }
        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        Habit habit = createHabitDto.ToEntity();
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
        var habitDto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit([FromRoute] string id, [FromBody] UpdateHabitDto updateHabitDto)
    {
        Habit habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
        {
            return NotFound($"Habit with ID {id} not found.");
        }
        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit([FromRoute]string id, [FromBody]JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
        {
            return NotFound($"Habit with ID {id} not found.");
        }
        var habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);
        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }
        habit.Name = habitDto.Name; 
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit([FromRoute] string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent(); 
    }
}
