using ITI.Resturant.Management.Application.DTOs.Menu;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using ITI.Resturant.Management.Application.ExternalServices.Contracts;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _categoryService;
        private readonly IFileUploadService? _fileUpload;
        private readonly IImageUrlResolver _imageResolver;

        public MenuItemsController(IMenuService menuService, IMenuCategoryService categoryService, IFileUploadService? fileUpload = null, IImageUrlResolver? imageResolver = null)
        {
            _menuService = menuService;
            _categoryService = categoryService;
            _fileUpload = fileUpload;
            _imageResolver = imageResolver ?? new FallbackImageUrlResolver();
        }

        private class FallbackImageUrlResolver : IImageUrlResolver
        {
            public string Resolve(string? imageUrl)
            {
                if (string.IsNullOrWhiteSpace(imageUrl)) return string.Empty;
                return imageUrl;
            }
        }

        // Main management view - PRIMARY ACTION
        [HttpGet]
        public async Task<IActionResult> ManageMenu(int page = 1, int pageSize = 20, string? search = null, int? categoryFilter = null)
        {
            var items = (await _menuService.GetAllMenuItemsAsync()).ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(i => (i.Name ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                                        || (i.Description ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (categoryFilter.HasValue)
            {
                items = items.Where(i => i.CategoryId == categoryFilter.Value).ToList();
            }

            foreach (var it in items)
            {
                it.ImageUrl = _imageResolver.Resolve(it.ImageUrl);
            }

            ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Search"] = search ?? string.Empty;
            ViewData["CategoryFilter"] = categoryFilter;

            return View(items);
        }

        // Alternative index view (API-style)
        [HttpGet("api/index", Name = "Admin_MenuItems_Index")]
        public async Task<IActionResult> Index()
        {
            var items = (await _menuService.GetAllMenuItemsAsync()).ToList();
            foreach (var it in items)
            {
                it.ImageUrl = _imageResolver.Resolve(it.ImageUrl);
            }
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(items);
        }

        // Create - GET
        [HttpGet]
        public async Task<IActionResult> CreateMenu()
        {
            ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
            return View(new MenuItemDto());
        }

        // Create - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenu(IFormCollection form)
        {
            try
            {
                var name = form["Name"].ToString();
                var description = form["Description"].ToString();
                decimal.TryParse(form["Price"], out var price);
                int.TryParse(form["CategoryId"], out var categoryId);
                int.TryParse(form["PreparationTimeInMinutes"], out var prep);
                var isAvailable = form["IsAvailable"].ToString().ToLower() == "true" || form["IsAvailable"].ToString() == "on";

                if (string.IsNullOrWhiteSpace(name))
                {
                    ModelState.AddModelError("Name", "Name is required.");
                    ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
                    return View();
                }

                string imageUrl = string.Empty;
                if (form.Files != null && form.Files.Count > 0 && _fileUpload != null)
                {
                    var file = form.Files[0];
                    imageUrl = await _fileUpload.UploadImageAsync(file, IFileUploadService.MenuImgsFolder);
                }

                var item = new MenuItem
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    CategoryId = categoryId,
                    PreparationTimeInMinutes = prep,
                    IsAvailable = isAvailable,
                    ImageUrl = imageUrl
                };

                await _menuService.CreateMenuItemAsync(item);
                TempData["Success"] = "Menu item created successfully.";
                return RedirectToAction(nameof(ManageMenu));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating menu item: {ex.Message}";
                ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
                return View();
            }
        }

        // Edit - GET
        [HttpGet]
        public async Task<IActionResult> EditMenu(int id)
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null) return NotFound();

            ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();

            var dto = new MenuItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                CategoryId = item.CategoryId,
                PreparationTimeInMinutes = item.PreparationTimeInMinutes,
                IsAvailable = item.IsAvailable,
                ImageUrl = _imageResolver.Resolve(item.ImageUrl)
            };

            return View(dto);
        }

        // Edit - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> EditMenu(int id, IFormCollection form)
        {
            try
            {
                var item = await _menuService.GetMenuItemByIdAsync(id);
                if (item == null) return NotFound();

                var name = form["Name"].ToString();
                var description = form["Description"].ToString();
                decimal.TryParse(form["Price"], out var price);
                int.TryParse(form["CategoryId"], out var categoryId);
                int.TryParse(form["PreparationTimeInMinutes"], out var prep);
                var isAvailable = form["IsAvailable"].ToString().ToLower() == "true" || form["IsAvailable"].ToString() == "on";

                if (string.IsNullOrWhiteSpace(name))
                {
                    ModelState.AddModelError("Name", "Name is required.");
                    ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
                    return View();
                }

                item.Name = name;
                item.Description = description;
                item.Price = price;
                item.CategoryId = categoryId;
                item.PreparationTimeInMinutes = prep;
                item.IsAvailable = isAvailable;

                string imageUrl = string.Empty;
                if (form.Files != null && form.Files.Count > 0 && _fileUpload != null)
                {
                    var file = form.Files.First();
                    imageUrl = await _fileUpload.UploadImageAsync(file, IFileUploadService.MenuImgsFolder);
                    item.ImageUrl = imageUrl;
                }

                await _menuService.UpdateMenuItemAsync(item);
                TempData["Success"] = "Menu item updated successfully.";
                return RedirectToAction(nameof(ManageMenu));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating menu item: {ex.Message}";
                ViewData["Categories"] = (await _categoryService.GetAllAsync()).ToList();
                return View();
            }
        }

        // Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null) return NotFound();

            item.ImageUrl = _imageResolver.Resolve(item.ImageUrl);

            return View(item);
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenu(IFormCollection form)
        {
            if (!int.TryParse(form["id"], out var id)) return BadRequest();
            var ok = await _menuService.DeleteMenuItemAsync(id);
            if (!ok)
            {
                TempData["Error"] = "Menu item not found or could not be deleted.";
                return NotFound();
            }
            TempData["Success"] = "Menu item deleted successfully.";
            return RedirectToAction("ManageMenu");
        }

        // Bulk Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0) return RedirectToAction("ManageMenu");
            await _menuService.BulkDeleteAsync(ids);
            TempData["Success"] = $"Deleted {ids.Length} item(s) successfully.";
            return RedirectToAction("ManageMenu");
        }

        // Toggle Availability - FIXED
        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                var item = await _menuService.GetMenuItemByIdAsync(id);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                item.IsAvailable = !item.IsAvailable;
                var result = await _menuService.UpdateMenuItemAsync(item);

                return Json(new
                {
                    success = result,
                    isAvailable = item.IsAvailable,
                    message = item.IsAvailable ? "Item is now available" : "Item is now unavailable"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // API Endpoints - Keep for backward compatibility
        [HttpGet("api/available")]
        public async Task<IActionResult> GetAvailable()
        {
            var items = await _menuService.GetAvailableMenuItemsAsync();
            var projected = items.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                description = i.Description,
                price = i.Price,
                categoryId = i.CategoryId,
                preparationTimeInMinutes = i.PreparationTimeInMinutes,
                isAvailable = i.IsAvailable,
                imageUrl = _imageResolver.Resolve(i.ImageUrl)
            });
            return Json(projected);
        }

        [HttpGet("api/item/{id}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var item = await _menuService.GetMenuItemByIdAsync(id);
            if (item == null) return NotFound();
            return Json(new
            {
                id = item.Id,
                name = item.Name,
                description = item.Description,
                price = item.Price,
                categoryId = item.CategoryId,
                preparationTimeInMinutes = item.PreparationTimeInMinutes,
                isAvailable = item.IsAvailable,
                imageUrl = _imageResolver.Resolve(item.ImageUrl)
            });
        }

        // Legacy Create/Edit actions for API routes
        [HttpGet("api/create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(new MenuItemDto());
        }

        [HttpPost("api/create")]
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

        [HttpGet("api/edit/{id:int}")]
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
                ImageUrl = _imageResolver.Resolve(menuItem.ImageUrl)
            };

            return View(dto);
        }

        [HttpPost("api/edit/{id:int}")]
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

        [HttpGet("api/delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();
            return View(menuItem);
        }

        [HttpPost("api/delete/{id:int}")]
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

        // Generic Save endpoint for AJAX
        [HttpPost("api/save")]
        [RequestSizeLimit(10_000_000)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(IFormCollection form)
        {
            try
            {
                var id = 0;
                int.TryParse(form["Id"], out id);
                var name = form["Name"].ToString();
                var description = form["Description"].ToString();
                decimal.TryParse(form["Price"], out var price);
                int.TryParse(form["CategoryId"], out var categoryId);
                int.TryParse(form["PreparationTimeInMinutes"], out var prep);
                var isAvailable = form["IsAvailable"].Count > 0;

                string imageUrl = string.Empty;
                if (form.Files != null && form.Files.Count > 0 && _fileUpload != null)
                {
                    var file = form.Files[0];
                    imageUrl = await _fileUpload.UploadImageAsync(file, IFileUploadService.MenuImgsFolder);
                }

                MenuItem item;
                if (id > 0)
                {
                    item = await _menuService.GetMenuItemByIdAsync(id) ?? new MenuItem();
                    item.Name = name;
                    item.Description = description;
                    item.Price = price;
                    item.CategoryId = categoryId;
                    item.PreparationTimeInMinutes = prep;
                    item.IsAvailable = isAvailable;
                    if (!string.IsNullOrEmpty(imageUrl)) item.ImageUrl = imageUrl;
                    await _menuService.UpdateMenuItemAsync(item);
                }
                else
                {
                    item = new MenuItem
                    {
                        Name = name,
                        Description = description,
                        Price = price,
                        CategoryId = categoryId,
                        PreparationTimeInMinutes = prep,
                        IsAvailable = isAvailable,
                        ImageUrl = imageUrl
                    };
                    var newId = await _menuService.CreateMenuItemAsync(item);
                    item.Id = newId;
                }

                return Json(new { id = item.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}