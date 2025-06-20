namespace StaffHabit.Api.DTOs.Tags;

public sealed class CreateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
