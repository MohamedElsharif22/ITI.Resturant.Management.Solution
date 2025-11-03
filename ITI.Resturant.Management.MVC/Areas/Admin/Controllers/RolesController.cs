using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITI.Resturant.Management.Domain.Identity;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Role name is required.");
                return View();
            }

            var exists = await _roleManager.RoleExistsAsync(name.Trim());
            if (exists)
            {
                ModelState.AddModelError("Name", "Role already exists.");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(name.Trim()));
            if (result.Succeeded) return RedirectToAction(nameof(Index));

            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var users = await _userManager.Users.ToListAsync();
            var members = new List<ApplicationUser>();
            foreach (var u in users)
            {
                if (await _userManager.IsInRoleAsync(u, role.Name)) members.Add(u);
            }

            ViewBag.Members = members;
            ViewBag.AllUsers = users;
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string name)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Role name is required.");
                ViewBag.Members = new List<ApplicationUser>();
                return View(role);
            }

            role.Name = name.Trim();
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded) return RedirectToAction(nameof(Index));

            foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            ViewBag.Members = new List<ApplicationUser>();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded) return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Unable to delete role.");
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return View("Index", roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToRole(string userId, string roleId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (user == null || role == null) return NotFound();

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (user == null || role == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));
            return Ok(new { success = true });
        }
    }
}
