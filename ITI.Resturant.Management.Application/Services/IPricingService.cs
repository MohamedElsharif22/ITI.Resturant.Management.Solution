using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IPricingService
    {
        decimal ApplyHappyHourDiscount(decimal amount, DateTime orderTime);
        decimal ApplyBulkDiscount(decimal amount);
        decimal CalculateTax(decimal amount);
        decimal CalculateFinalPrice(decimal subtotal, IEnumerable<string> appliedDiscounts);
    }
}
