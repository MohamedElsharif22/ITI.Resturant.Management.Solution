using ITI.Resturant.Management.Domain.Entities.Order_;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Domain.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Repositories.Contracts
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    }
}
