using ITI.Resturant.Management.Application.DependancyInjection;
using ITI.Resturant.Management.Infrastructure.DependancyInjection;
using ITI.Resturant.Management.MVC.Helpers;

namespace ITI.Resturant.Management.MVC
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddInfrastructureServices();
            builder.Services.AddDbContextServices(builder.Configuration);
            builder.Services.AddApplicationServices();


            var app = builder.Build();

            await app.MiagrateAndSeedDatabasesAsync();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
