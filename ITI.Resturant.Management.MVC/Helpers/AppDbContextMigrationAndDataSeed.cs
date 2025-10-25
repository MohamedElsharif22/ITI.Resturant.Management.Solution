using ITI.Resturant.Management.Domain.Identity;
using ITI.Resturant.Management.Infrastructure._Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITI.Resturant.Management.MVC.Helpers
{
    public static class AppDbContextMigrationAndDataSeed
    {
        public static async Task<WebApplication> MiagrateAndSeedDatabasesAsync(this WebApplication webApplication)
        {
            using var scope = webApplication.Services.CreateScope();

            var services = scope.ServiceProvider;

            var _bywayDbContext = services.GetRequiredService<AppDbContext>();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await _bywayDbContext.Database.MigrateAsync();

                var _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var _userManeger = services.GetRequiredService<UserManager<ApplicationUser>>();
                var _context = services.GetRequiredService<AppDbContext>();

                await AppDbContextSeed.SeedIdentityDataAsync(_userManeger, _roleManager);
                await AppDbContextSeed.SeedInitialDataAsync(_context);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();

                logger.LogError(ex, "an error has been occured while running migration!");
            }

            return webApplication;
        }
    }
}
