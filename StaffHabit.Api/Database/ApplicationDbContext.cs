using Microsoft.EntityFrameworkCore;

namespace StaffHabit.Api.Database;
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) : base(dbContext) 
    { 
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
    }
}

 
