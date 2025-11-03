using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMenuService _menuService;

        public HomeController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = (await _menuService.GetAvailableMenuItemsAsync()).ToList();
            var categories = (await _menuService.GetActiveCategoriesAsync()).ToList();

            var model = new HomeViewModel
            {
                FeaturedMenuItems = items.Take(6).ToList(),
                Categories = categories
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
