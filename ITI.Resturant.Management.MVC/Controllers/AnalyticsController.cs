using ITI.Resturant.Management.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null)
        {
            var end = to ?? DateTime.Today;
            var start = from ?? end.AddDays(-7);

            var total = await _analytics.GetTotalSalesAsync(start, end);
            var daily = await _analytics.GetDailySalesAsync(start, end);
            var top = await _analytics.GetTopSellingItemsAsync(start, end, 10);

            ViewData["TotalSales"] = total;
            ViewData["DailySales"] = daily;
            ViewData["TopItems"] = top;

            return View();
        }
    }
}
