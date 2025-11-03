using ITI.Resturant.Management.Application.DTOs;
using ITI.Resturant.Management.Application.DTOs.Menu;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace ITI.Resturant.Management.MVC.Controllers
{
    [Route("menu")]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _categoryService;

        public MenuController(IMenuService menuService, IMenuCategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        [HttpGet("", Name = "Menu_Index")]
        public async Task<IActionResult> Index()
        {
            var categories = (await _categoryService.GetActiveCategories()).ToList();
            var items = (await _menuService.GetAllMenuItemsAsync()).ToList();

            var model = categories.Select(c => new
            {
                Category = c,
                Items = items.Where(i => i.CategoryId == c.Id && i.IsAvailable)
            }).ToList();

            return View(model);
        }

        [HttpGet("details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();
            return View(menuItem);
        }

        [HttpGet("category/{categoryName}")]
        public async Task<IActionResult> ByCategory(string categoryName)
        {
            var cats = await _categoryService.GetAllAsync();
            var cat = cats.FirstOrDefault(c => string.Equals(c.Name, categoryName, System.StringComparison.OrdinalIgnoreCase));
            if (cat == null) return NotFound();

            var items = await _menuService.GetByCategoryAsync(cat.Id);
            var available = items.Where(i => i.IsAvailable);
            return View("Index", new[] { new { Category = cat, Items = available } });
        }
    }
}
