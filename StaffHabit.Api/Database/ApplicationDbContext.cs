using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Entities;

namespace StaffHabit.Api.Database;
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) : base(dbContext) 
    { 
    }

    public DbSet<Habit> Habits { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<HabitTag> HabitTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);


        modelBuilder.Entity<Habit>().HasData(
            new Habit
            {
                Id = "h_6c00a976-04e3-4ea9-963b-36806b9ca2a2",
                Name = "Test Habit",
                Description = "This is a test habit",
                Type = HabitType.Binary,
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                CreatedAtUtc = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = null,
                LastCompletedAtUtc = null
            },

            new Habit
            {
                Id = "h_bc2ed4a9-c28c-4bd8-b60e-35d4b70ef630",
                Name = "Test Habit Two",
                Description = "This is a test habit two",
                Type = HabitType.Binary,
                Status = HabitStatus.Ongoing,
                IsArchived = false,
                EndDate = null,
                CreatedAtUtc = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = null,
                LastCompletedAtUtc = null
            }
        );

        modelBuilder.Entity<Habit>().OwnsOne(h => h.Frequency).HasData(

            new
            {
                HabitId = "h_6c00a976-04e3-4ea9-963b-36806b9ca2a2",
                Type = FrequencyType.Weekly,
                TimesPeriod = 1
            },

            new
            {
                HabitId = "h_bc2ed4a9-c28c-4bd8-b60e-35d4b70ef630",
                Type = FrequencyType.Daily,
                TimesPeriod = 1
            }
        );

        modelBuilder.Entity<Habit>().OwnsOne(h => h.Target).HasData(

            new
            {
                HabitId = "h_6c00a976-04e3-4ea9-963b-36806b9ca2a2",
                Value = 2,
                Unit = "times"
            },

            new
            {
                HabitId = "h_bc2ed4a9-c28c-4bd8-b60e-35d4b70ef630",
                Value = 3,
                Unit = "times"
            }
        );

        modelBuilder.Entity<Habit>().OwnsOne(h => h.Milestone).HasData(

            new 
            {
                HabitId = "h_6c00a976-04e3-4ea9-963b-36806b9ca2a2",
                Target = 10,
                Current = 0
            },

            new
            {
                HabitId = "h_bc2ed4a9-c28c-4bd8-b60e-35d4b70ef630",
                Target = 4,
                Current = 2
            }
        );
    }
}

 
