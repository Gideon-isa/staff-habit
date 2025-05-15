using Microsoft.EntityFrameworkCore;
using StaffHabit.Api.Database;

namespace StaffHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migration applied successfully.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occured while applying database migrations");
            Console.WriteLine(e);
            throw;
        }
    }
}
