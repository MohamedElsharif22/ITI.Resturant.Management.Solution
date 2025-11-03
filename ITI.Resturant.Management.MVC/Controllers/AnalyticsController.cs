using ITI.Resturant.Management.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace ITI.Resturant.Management.MVC.Controllers
{
    [Route("analytics")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analytics;
        private readonly IOrderService _orders;

        public AnalyticsController(IAnalyticsService analytics, IOrderService orders)
        {
            _analytics = analytics;
            _orders = orders;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var f = from ?? DateTime.Today;
            var t = to ?? DateTime.Today.AddDays(1).AddTicks(-1);

            var daily = await _analytics.GetTotalSalesAsync(f, t);
            var top = await _analytics.GetTopSellingItemsAsync(f, t, 10);
            var byCategory = await _analytics.GetSalesByCategoryAsync(f, t);

            ViewBag.DailySales = daily;
            ViewBag.TopItems = top;
            ViewBag.ByCategory = byCategory;
            ViewBag.From = f;
            ViewBag.To = t;

            return View();
        }
    }
}
