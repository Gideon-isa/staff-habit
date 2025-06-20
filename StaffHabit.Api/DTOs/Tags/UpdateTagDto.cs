namespace StaffHabit.Api.DTOs.Tags;

public sealed class UpdateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
