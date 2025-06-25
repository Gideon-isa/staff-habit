using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Database;
using StaffHabit.Api.DTOs.HabitTag;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.Controllers;

[ApiController]
[Route("habits/{habitId}/tags")]
public sealed class HabitTagsController(ApplicationDbContext dbContext) : ControllerBase
{

    [HttpPut]
    public async Task<ActionResult> UpserHabitTags([FromRoute] string habitId, [FromBody] UpsertHabitTagsDto upsertHabitTagsDto)
    {
        Habit? habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        if (habit is null)
        {
            return NotFound();
        }
        var currentTagIds = habit
            .HabitTags
            .Select(h => h.TagId).ToHashSet();

        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        { 
            return NoContent();
        }

        List<string> existingTagIds = await dbContext
            .Tags
            .Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id))
            .Select(t => t.Id).ToListAsync();

        if (existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("Some of the provided tag IDs do not exist.");
        }
        // removing ids which are not present in the upsertHabitTagsDto request
        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));

        string[] tagsIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();
        habit.HabitTags.AddRange(tagsIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc = DateTime.UtcNow,
        }));
        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags.SingleOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);
        if (habitTag is null)
        {
            return NotFound();
        }
        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

}
