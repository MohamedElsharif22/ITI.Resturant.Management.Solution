using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IAnalyticsService
    {
        Task<decimal> GetTotalSalesAsync(DateTime from, DateTime to);
        Task<IDictionary<int, decimal>> GetSalesByCategoryAsync(DateTime from, DateTime to);
        Task<IDictionary<int, int>> GetTopSellingItemsAsync(DateTime from, DateTime to, int top = 10);
        // Returns a map of date (yyyy-MM-dd) => sales total for that date
        Task<IDictionary<string, decimal>> GetDailySalesAsync(DateTime from, DateTime to);
    }
}
