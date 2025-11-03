using ITI.Resturant.Management.Domain.Entities.Cart_;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Repositories.Contracts
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(string id);
        Task<Cart?> UpdateCartAsync(Cart cart);
        void DeleteCart(string cartId);
    }
}