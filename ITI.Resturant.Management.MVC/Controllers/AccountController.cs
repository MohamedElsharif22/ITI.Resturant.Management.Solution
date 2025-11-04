using ITI.Resturant.Management.Application.DTOs.Account;
using ITI.Resturant.Management.Application.DTOs.Email;
using ITI.Resturant.Management.Application.ExternalServices.Contracts;
using ITI.Resturant.Management.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(dto);

            ApplicationUser? user = null;

            user = await _userManager.FindByEmailAsync(dto.EmailorUserName);

            user ??= await _userManager.FindByNameAsync(dto.EmailorUserName);
            

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

            // Handle return URL
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // role check - redirect admin to admin dashboard
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        // GET Register - return the view for registration
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterDto());
        }

        // Update Register method similarly
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto, string? returnUrl = null)
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

            // Handle return URL
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Profile action to serve Views/Account/Profile.cshtml
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // If user not found, redirect to login
                return RedirectToAction("Login");
            }

            var model = new EditProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName
            };

            return View(model);
        }

        // GET: Forget Password
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordDto());
        }

        // POST: Forget Password - send token via email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Do not reveal that the user does not exist
                TempData["Info"] = "If an account with that email exists, a reset link was sent.";
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build reset URL
            var resetUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            // Create email request
            var email = new EmailRequest
            {
                ToEmail = user.Email ?? string.Empty,
                ToName = user.UserName,
                Subject = "Password Reset - Restaurant Management",
                Body = $"<p>Hello {user.UserName},</p><p>You requested a password reset. Click the link below to reset your password:</p><p><a href=\"{resetUrl}\">Reset Password</a></p><p>If you did not request this, please ignore this email.</p>",
                IsHtml = true
            };

            try
            {
                await _emailService.SendEmailAsync(email);
            }
            catch
            {
                // Do not reveal details to the user; set a generic info message
                TempData["Info"] = "If an account with that email exists, a reset link was sent.";
                return RedirectToAction(nameof(Login));
            }

            TempData["Info"] = "If an account with that email exists, a reset link was sent.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            var model = new ResetPasswordDto { UserId = userId ?? string.Empty, Token = token ?? string.Empty };
            return View(model);
        }

        // POST: Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user.");
                return View(dto);
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Password reset successfully. You can now login.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Edit Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var dto = new EditProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName
            };

            return View(dto);
        }

        // POST: Edit Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditProfileDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            // Optionally update username
            if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
            {
                var setUserName = await _userManager.SetUserNameAsync(user, dto.UserName);
                if (!setUserName.Succeeded)
                {
                    foreach (var e in setUserName.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(dto);
                }
            }

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var e in update.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // GET: Change Password
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDto());
        }

        // POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(dto);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        // Optional: Logout action referenced by the Profile view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
