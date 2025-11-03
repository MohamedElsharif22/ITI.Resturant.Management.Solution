using ITI.Resturant.Management.Domain.Entities.Order_;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IDiscountService
    {
        decimal CalculateDiscount(Order order);
        bool IsHappyHour();
    }

    public class DiscountService : IDiscountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const decimal HAPPY_HOUR_DISCOUNT = 0.20m; // 20% off
        private const decimal BULK_ORDER_DISCOUNT = 0.10m; // 10% off
        private const decimal STUDENT_DISCOUNT = 0.15m; // 15% off
        private const decimal SENIOR_DISCOUNT = 0.15m; // 15% off
        private const decimal BULK_ORDER_THRESHOLD = 100m;

        public DiscountService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsHappyHour()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Items["IsHappyHour"] as bool? ?? false;
        }

        public decimal CalculateDiscount(Order order)
        {
            if (order == null || order.Subtotal <= 0)
                return 0;

            decimal totalDiscount = 0;
            decimal remainingAmount = order.Subtotal;

            // Apply happy hour discount first (if applicable)
            if (IsHappyHour())
            {
                var happyHourDiscount = remainingAmount * HAPPY_HOUR_DISCOUNT;
                totalDiscount += happyHourDiscount;
                remainingAmount -= happyHourDiscount;
            }

            // Apply bulk order discount next
            if (order.Subtotal > BULK_ORDER_THRESHOLD)
            {
                var bulkDiscount = remainingAmount * BULK_ORDER_DISCOUNT;
                totalDiscount += bulkDiscount;
                remainingAmount -= bulkDiscount;
            }

            // Apply student/senior discount last (if applicable)
            if (order.CustomerDiscountType == Domain.Entities.Enums.CustomerDiscountType.Student ||
                order.CustomerDiscountType == Domain.Entities.Enums.CustomerDiscountType.Senior)
            {
                var personalDiscount = remainingAmount * 
                    (order.CustomerDiscountType == Domain.Entities.Enums.CustomerDiscountType.Student ? 
                    STUDENT_DISCOUNT : SENIOR_DISCOUNT);
                totalDiscount += personalDiscount;
            }

            // Ensure total discount doesn't exceed order subtotal
            return Math.Min(totalDiscount, order.Subtotal);
        }
    }

    // Add extension method for service registration
    public static class DiscountServiceExtensions
    {
        public static IServiceCollection AddDiscountService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IDiscountService, DiscountService>();
            return services;
        }
    }
}