using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IAnalyticsService analyticsService, UserManager<ApplicationUser> userManager)
        {
            _analyticsService = analyticsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null)
        {
            var toDate = to?.Date ?? DateTime.Today;
            var fromDate = from?.Date ?? toDate.AddDays(-6);

            var totalSales = await _analyticsService.GetTotalSalesAsync(fromDate, toDate);
            var salesByCategory = await _analyticsService.GetSalesByCategoryAsync(fromDate, toDate);
            var topItems = await _analyticsService.GetTopSellingItemsAsync(fromDate, toDate, 5);
            var dailySales = await _analyticsService.GetDailySalesAsync(fromDate, toDate);

            ViewBag.TotalRevenue = totalSales;
            ViewBag.ByCategory = salesByCategory;
            ViewBag.TopItems = topItems;
            ViewBag.DailySales = dailySales;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;

            ViewBag.RegisteredUsers = await _userManager.Users.CountAsync();

            // return the area view by convention (Areas/Admin/Views/Dashboard/Index.cshtml)
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}
