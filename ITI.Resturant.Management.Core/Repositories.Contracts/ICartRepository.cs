using ITI.Resturant.Management.Domain.Entities.Cart_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Repositories.Contracts
{
    public interface ICartRepository
    {
        const decimal TaxRate = 0.15m;
        Task<Cart?> GetCartAsync(string cartId);
        Task<Cart?> UpdateCartAsync(Cart cart);
        public void DeleteCart(string cartId);

    }
}
