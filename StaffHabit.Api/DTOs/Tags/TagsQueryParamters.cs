using Microsoft.AspNetCore.Mvc;

namespace StaffHabit.Api.DTOs.Tags;

public class TagsQueryParamters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
