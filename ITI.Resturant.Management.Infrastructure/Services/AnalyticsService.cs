using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Infrastructure._Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext context, IMemoryCache cache, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime from, DateTime to)
        {
            var key = $"totalsales_{from:yyyyMMdd}_{to:yyyyMMdd}";
            if (_cache.TryGetValue(key, out decimal cached)) return cached;

            var total = await _context.Orders
                .Where(o => !o.IsDeleted && o.OrderDate >= from && o.OrderDate <= to)
                .SumAsync(o => (decimal?)o.Total) ?? 0m;

            _cache.Set(key, total, TimeSpan.FromMinutes(5));
            return total;
        }

        public async Task<IDictionary<int, decimal>> GetSalesByCategoryAsync(DateTime from, DateTime to)
        {
            var key = $"salesbycat_{from:yyyyMMdd}_{to:yyyyMMdd}";
            if (_cache.TryGetValue(key, out IDictionary<int, decimal> cached)) return cached;

            var dict = await _context.OrderItems
                .Join(_context.MenuItems, oi => oi.MenuItemId, mi => mi.Id, (oi, mi) => new { oi, mi })
                .Join(_context.Orders, x => x.oi.OrderId, o => o.Id, (x, o) => new { x.oi, x.mi, o })
                .Where(x => !x.o.IsDeleted && x.o.OrderDate >= from && x.o.OrderDate <= to)
                .GroupBy(x => x.mi.CategoryId)
                .Select(g => new { CategoryId = g.Key, Sales = g.Sum(x => x.oi.Subtotal) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Sales);

            _cache.Set(key, dict, TimeSpan.FromMinutes(5));
            return dict;
        }

        public async Task<IDictionary<int, int>> GetTopSellingItemsAsync(DateTime from, DateTime to, int top = 10)
        {
            var key = $"topselling_{from:yyyyMMdd}_{to:yyyyMMdd}_{top}";
            if (_cache.TryGetValue(key, out IDictionary<int, int> cached)) return cached;

            var dict = await _context.OrderItems
                .Join(_context.Orders, oi => oi.OrderId, o => o.Id, (oi, o) => new { oi, o })
                .Where(x => !x.o.IsDeleted && x.o.OrderDate >= from && x.o.OrderDate <= to)
                .GroupBy(x => x.oi.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, Quantity = g.Sum(x => x.oi.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(top)
                .ToDictionaryAsync(x => x.MenuItemId, x => x.Quantity);

            _cache.Set(key, dict, TimeSpan.FromMinutes(5));
            return dict;
        }

        public async Task<IDictionary<string, decimal>> GetDailySalesAsync(DateTime from, DateTime to)
        {
            var key = $"dailysales_{from:yyyyMMdd}_{to:yyyyMMdd}";
            if (_cache.TryGetValue(key, out IDictionary<string, decimal> cached)) return cached;

            // Build series of dates
            var days = Enumerable.Range(0, (to.Date - from.Date).Days + 1)
                                 .Select(i => from.Date.AddDays(i))
                                 .ToList();

            var sales = await _context.Orders
                .Where(o => !o.IsDeleted && o.OrderDate >= from && o.OrderDate <= to)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.Total) })
                .ToListAsync();

            var dict = days.ToDictionary(d => d.ToString("yyyy-MM-dd"), d => (decimal)0);
            foreach (var s in sales)
            {
                var k = s.Date.ToString("yyyy-MM-dd");
                if (dict.ContainsKey(k)) dict[k] = s.Total;
            }

            _cache.Set(key, dict, TimeSpan.FromMinutes(5));
            return dict;
        }
    }
}
