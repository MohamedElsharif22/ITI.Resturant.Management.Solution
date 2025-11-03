using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly IMenuCategoryService _categoryService;

        public CategoriesController(IMenuCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories()
        {
            var categories = (await _categoryService.GetAllAsync()).ToList();
            return View(categories); // use area view by convention
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new MenuCategory()); // use area view by convention
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([FromForm] MenuCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            await _categoryService.AddAsync(model);
            return RedirectToAction(nameof(ManageCategories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, [FromForm] MenuCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.IconClass = model.IconClass;
            await _categoryService.UpdateAsync(existing);
            return RedirectToAction(nameof(ManageCategories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(ManageCategories));
        }
    }
}
