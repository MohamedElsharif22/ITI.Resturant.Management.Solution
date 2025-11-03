using AutoMapper;
using ITI.Resturant.Management.Application.DTOs.Account;
using ITI.Resturant.Management.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AccountService(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var user = _mapper.Map<ApplicationUser>(dto);
            
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return false;

            // assign default 'Customer' role
            await _userManager.AddToRoleAsync(user, "Customer");
            return true;
        }

        public async Task<bool> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, dto.Password);
        }

        public Task LogoutAsync()
        {
            // Logout is handled in MVC layer via SignInManager; keep as no-op for interface compliance
            return Task.CompletedTask;
        }
    }
}
