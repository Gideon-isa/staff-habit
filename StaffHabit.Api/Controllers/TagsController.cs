using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Database;
using StaffHabit.Api.DTOs.Tags;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.Controllers;

[ApiController]
[Route("tags")]
public sealed class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetHabits()
    {
        List<TagDto> tags = await dbContext.Tags.Select(TagsQueries.ProjectToDto()).ToListAsync();

        var tagsCollectionDto = new TagsCollectionDto
        {
            Data = tags
        };
        return Ok(tagsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag([FromRoute] string id)
    {
        TagDto tag = await dbContext.Tags
            .Where(t => t.Id == id)
            .Select(TagsQueries.ProjectToDto()).FirstOrDefaultAsync();
        if (tag == null)
        {
            return NotFound($"Tag with ID {id} not found.");
        }
        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto, [FromServices] IValidator<CreateTagDto> validator, ProblemDetailsFactory problemDetailsFactory)
    {
        ValidationResult validationResult = await validator.ValidateAsync(createTagDto);
        if (!validationResult.IsValid)
        {
            var problem = problemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }
        Tag tag = createTagDto.ToEntity();
        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(detail: $"Tag with name '{tag.Name}' already exists.", statusCode: StatusCodes.Status409Conflict);
        } 
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        var tagDto = tag.ToDto();
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag([FromRoute] string id, [FromBody] UpdateTagDto updateTagDto)
    {
        Tag tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return NotFound($"Tag with ID {id} not found.");
        }
        tag.UpdateFromDto(updateTagDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult> PatchTag([FromRoute] string id, [FromBody] JsonPatchDocument<TagDto> patchDocument)
    {
        Tag tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return NotFound($"Tag with ID {id} not found.");
        }
        var tagDto = tag.ToDto();
        patchDocument.ApplyTo(tagDto, ModelState);

        if (!TryValidateModel(tagDto))
        {
            return ValidationProblem(ModelState);
        }
        tag.Name = tagDto.Name;
        tag.Description = tagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag([FromRoute] string id)
    {
        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null)
        {
            return NotFound($"Tag with ID {id} not found.");
        }
        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
