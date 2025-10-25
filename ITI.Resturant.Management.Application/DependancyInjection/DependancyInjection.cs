using ITI.Resturant.Management.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.DependancyInjection
{
    public static partial class DependancyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper((e) => { }, typeof(MappingProfile).Assembly);
            return services;
        }
    }
}
