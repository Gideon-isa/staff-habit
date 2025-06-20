using System.Linq.Expressions;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.DTOs.Tags;

internal static class TagsQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto() => t => new TagDto
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        CreatedAtUtc = t.CreatedAtUtc,
        UpdatedAtUtc = t.UpdatedAtUtc
    };
}
