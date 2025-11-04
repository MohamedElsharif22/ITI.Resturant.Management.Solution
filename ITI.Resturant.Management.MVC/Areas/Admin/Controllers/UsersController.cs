using ITI.Resturant.Management.Application.DTOs.Admin;
using ITI.Resturant.Management.Application.DTOs.Email;
using ITI.Resturant.Management.Application.ExternalServices.Contracts;
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
        private readonly IEmailService _emailService;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
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

            // Generate a reset token and send a password reset email to the user
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            var email = new EmailRequest
            {
                ToEmail = user.Email ?? string.Empty,
                ToName = user.UserName,
                Subject = "Admin initiated password reset",
                Body = $"<p>Hello {user.UserName},</p><p>An administrator initiated a password reset for your account. Click the link below to reset your password:</p><p><a href=\"{resetUrl}\">Reset your password</a></p>",
                IsHtml = true
            };

            try
            {
                await _emailService.SendEmailAsync(email);
            }
            catch
            {
                return BadRequest(new { success = false, message = "Failed to send reset email." });
            }

            return Ok(new { success = true });
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
