using ITI.Resturant.Management.Application.DTOs.Admin;
using ITI.Resturant.Management.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > System.DateTimeOffset.Now
                })
                .ToListAsync();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                user.Roles = (await _userManager.GetRolesAsync(appUser)).ToList();
            }

            ViewData["AvailableRoles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userDto = new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > System.DateTimeOffset.Now,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };

            ViewData["AvailableRoles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(userDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(roles).ToList();
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            var rolesToAdd = roles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.Now.AddYears(100));
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tempPassword = "TempPass123!"; // In production, generate a random password
            
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            // Force password change on next login
            await _userManager.UpdateSecurityStampAsync(user);

            return Ok(new { success = true, tempPassword });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { success = true });
        }
    }
}
