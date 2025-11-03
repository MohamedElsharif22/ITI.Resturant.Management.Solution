using ITI.Resturant.Management.Application.DTOs.Account;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IAccountService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<bool> LoginAsync(LoginDto dto);
        Task LogoutAsync();
    }
}
