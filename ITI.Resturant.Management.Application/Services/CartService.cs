using ITI.Resturant.Management.Domain.Entities.Cart_;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(string cartId);
        Task<Cart?> UpdateCartAsync(Cart cart);
        void DeleteCart(string cartId);
    }

    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Cart?> GetCartAsync(string cartId)
        {
            return await _cartRepository.GetCartAsync(cartId);
        }

        public async Task<Cart?> UpdateCartAsync(Cart cart)
        {
            return await _cartRepository.UpdateCartAsync(cart);
        }

        public void DeleteCart(string cartId)
        {
            _cartRepository.DeleteCart(cartId);
        }
    }
}