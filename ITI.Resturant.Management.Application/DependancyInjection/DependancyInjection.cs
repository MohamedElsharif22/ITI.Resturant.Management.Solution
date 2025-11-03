using ITI.Resturant.Management.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITI.Resturant.Management.Application.Services;

namespace ITI.Resturant.Management.Application.DependancyInjection
{
    public static partial class DependancyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper((e) => { }, typeof(MappingProfile).Assembly);

            // Application services
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICartService, CartService>();
            // New services
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IMenuCategoryService, MenuCategoryService>();
            services.AddScoped<IPricingService, PricingService>();

            // Do NOT register implementation types from Infrastructure here (keep Clean Architecture)
            // Infrastructure project should register IAnalyticsService -> AnalyticsService
            return services;
        }
    }
}
