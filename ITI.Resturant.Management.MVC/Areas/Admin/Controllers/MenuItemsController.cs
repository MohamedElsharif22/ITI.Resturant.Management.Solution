using ITI.Resturant.Management.Application.DTOs.Menu;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _categoryService;

        public MenuItemsController(IMenuService menuService, IMenuCategoryService categoryService)
        {
            _menuService = menuService;
            _categoryService = categoryService;
        }

        [HttpGet("", Name = "Admin_MenuItems_Index")]
        public async Task<IActionResult> Index()
        {
            var items = await _menuService.GetAllMenuItemsAsync();
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(items);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(new MenuItemDto());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(dto);
            }

            var entity = new MenuItem
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                PreparationTimeInMinutes = dto.PreparationTimeInMinutes,
                IsAvailable = dto.IsAvailable,
                ImageUrl = dto.ImageUrl ?? string.Empty
            };

            await _menuService.CreateMenuItemAsync(entity);
            TempData["SuccessMessage"] = "Menu item created successfully.";
            return RedirectToRoute("Admin_MenuItems_Index");
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();

            ViewBag.Categories = await _categoryService.GetAllAsync();

            var dto = new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                CategoryId = menuItem.CategoryId,
                PreparationTimeInMinutes = menuItem.PreparationTimeInMinutes,
                IsAvailable = menuItem.IsAvailable,
                ImageUrl = menuItem.ImageUrl
            };

            return View(dto);
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(dto);
            }

            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();

            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description ?? string.Empty;
            menuItem.Price = dto.Price;
            menuItem.CategoryId = dto.CategoryId;
            menuItem.PreparationTimeInMinutes = dto.PreparationTimeInMinutes;
            menuItem.IsAvailable = dto.IsAvailable;
            menuItem.ImageUrl = dto.ImageUrl ?? string.Empty;

            if (await _menuService.UpdateMenuItemAsync(menuItem))
            {
                TempData["SuccessMessage"] = "Menu item updated successfully.";
                return RedirectToRoute("Admin_MenuItems_Index");
            }

            ModelState.AddModelError("", "Failed to update menu item.");
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(dto);
        }

        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();
            return View(menuItem);
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _menuService.DeleteMenuItemAsync(id))
            {
                TempData["SuccessMessage"] = "Menu item deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete menu item.";
            }
            return RedirectToRoute("Admin_MenuItems_Index");
        }

        [HttpPost("toggle-availability/{id:int}")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var result = await _menuService.ToggleAvailabilityAsync(id);
            return Json(new { success = result });
        }

        [HttpPost("bulk-delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] int[] ids)
        {
            await _menuService.BulkDeleteAsync(ids);
            return Json(new { success = true });
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var items = await _menuService.GetAvailableMenuItemsAsync();
            return Json(items);
        }
    }
}
