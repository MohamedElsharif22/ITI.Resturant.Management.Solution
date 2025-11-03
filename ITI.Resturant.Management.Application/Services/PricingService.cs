using System;
using System.Collections.Generic;
using System.Linq;

namespace ITI.Resturant.Management.Application.Services
{
    public class PricingService : IPricingService
    {
        private const decimal TaxRate = 0.085m;
        private const decimal HappyHourRate = 0.20m;
        private const decimal BulkRate = 0.10m;
        private const decimal BulkThreshold = 100m;

        public decimal ApplyHappyHourDiscount(decimal amount, DateTime orderTime)
        {
            var time = TimeOnly.FromDateTime(orderTime);
            var start = new TimeOnly(15, 0);
            var end = new TimeOnly(17, 0);
            if (time >= start && time <= end)
                return Math.Round(amount * HappyHourRate, 2);
            return 0m;
        }

        public decimal ApplyBulkDiscount(decimal amount)
        {
            if (amount > BulkThreshold)
                return Math.Round(amount * BulkRate, 2);
            return 0m;
        }

        public decimal CalculateTax(decimal amount)
        {
            return Math.Round(amount * TaxRate, 2);
        }

        public decimal CalculateFinalPrice(decimal subtotal, IEnumerable<string> appliedDiscounts)
        {
            decimal discount = 0m;
            foreach (var d in appliedDiscounts)
            {
                if (d == "HappyHour") discount += ApplyHappyHourDiscount(subtotal, DateTime.Now);
                if (d == "Bulk") discount += ApplyBulkDiscount(subtotal);
            }

            var tax = CalculateTax(subtotal - discount);
            var final = subtotal - discount + tax;
            return Math.Round(final, 2);
        }
    }
}
