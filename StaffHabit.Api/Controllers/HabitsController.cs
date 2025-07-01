using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Database;
using StaffHabit.Api.DTOs.Common;
using StaffHabit.Api.DTOs.Habits;
using StaffHabit.Api.Entities;
using StaffHabit.Api.Services;
using StaffHabit.Api.Services.Sorting;
using StaffHabit.Api.Services.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace StaffHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query, 
        SortMappingProvider sortMappingProvider, 
        DataShapingService dataShapingService)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Sort Field"
            );
        }

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest, 
                detail: $"The provided sort paramters are not valid: '{query.Fields}'"); 
        }
        query.Search ??= query.Search?.Trim().ToLower();
        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery =  dbContext
            .Habits
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower()
                        .Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());

        int totalCount = await habitsQuery.CountAsync();

        List<HabitDto> habits = await habitsQuery
            .Skip((query.Page -1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var PaginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(habits, query.Fields),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
        }; 
        return Ok(PaginationResult);
    }
     

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit([FromRoute] string id, [FromQuery] string? fields, DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort paramters are not valid: '{fields}'");
        }
        HabitWithTagsDto habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();

        if (habit == null)
        {
            return NotFound($"Habit with ID {id} not found.");
        }
        ExpandoObject shapedHabitDto =  dataShapingService.ShapeData(habit, fields);
        return Ok(shapedHabitDto);
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
