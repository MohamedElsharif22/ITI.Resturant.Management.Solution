using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _menuService.GetActiveCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
