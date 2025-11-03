using ITI.Resturant.Management.Application.DTOs.Account;
using ITI.Resturant.Management.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user = await _userManager.FindByEmailAsync(dto.Email);
            }

            if (user == null && !string.IsNullOrWhiteSpace(dto.UserName))
            {
                user = await _userManager.FindByNameAsync(dto.UserName);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(dto);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(dto);
            }

            // sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            // role check - redirect admin to admin dashboard
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // redirect to area Admin -> Dashboard controller Index
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(dto);
            }

            // assign default 'Customer' role if exists
            await _userManager.AddToRoleAsync(user, "Customer");

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            ViewData["Email"] = user.Email;
            ViewData["UserName"] = user.UserName;
            ViewData["FirstName"] = user.FirstName;
            ViewData["LastName"] = user.LastName;
            return View();
        }
    }
}
