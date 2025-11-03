using Byway.Application.Contracts.ExternalServices;
using ITI.Resturant.Management.Application.ExternalServices.Contracts;
using ITI.Resturant.Management.Application.Mapping;
using ITI.Resturant.Management.Domain.Identity;
using ITI.Resturant.Management.Infrastructure._Data;
using ITI.Resturant.Management.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Infrastructure.Repositories;
using ITI.Resturant.Management.Domain;
using ITI.Resturant.Management.Application.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ITI.Resturant.Management.Infrastructure.DependancyInjection
{
    public static partial class DependancyInjection
    {
        public static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configure Context Services
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
            {
                if (string.IsNullOrWhiteSpace(conn))
                {
                    // Fallback to in-memory for development when connection string not provided
                    options.UseInMemoryDatabase("AppDbInMemory");
                }
                else
                {
                    options.UseSqlServer(conn);
                    // Prevent EF Core from throwing on pending model changes during Migrate in dev
                    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                }
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            }).AddEntityFrameworkStores<AppDbContext>();

            return services;
        }
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // unit of work and repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddMemoryCache();
            services.AddScoped<ICartRepository, CartRepository>();

            // Configure order progression options
            services.Configure<OrderProgressionOptions>(options =>
            {
                options.MaxConcurrentOrders = 50;
                options.RetryDelaySeconds = 30;
                options.MaxRetries = 3;
            });

            // Hosted services and background workers - register as singleton for both interface and implementation
            services.AddSingleton<OrderProgressionHostedService>();
            services.AddSingleton<IOrderProgressionService>(sp => sp.GetRequiredService<OrderProgressionHostedService>());
            services.AddHostedService(sp => sp.GetRequiredService<OrderProgressionHostedService>());

            // Analytics service implementation
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // Application services
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IMenuCategoryService, MenuCategoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IPricingService, PricingService>();

            // File upload service
            services.AddScoped<IFileUploadService, FileUploadService>();

            // Image URL resolver
            services.AddSingleton<IImageUrlResolver, ImageUrlResolver>();

            return services;
        }
    }
}
