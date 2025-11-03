using ITI.Resturant.Management.Domain.Entities.Enums;
using ITI.Resturant.Management.Domain.Entities.Order_;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Infrastructure._Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _Context.Orders
                .Where(o => o.Status == status && !o.IsDeleted)
                .Include(o => o.OrderItems)
                .ToListAsync();
        }
    }
}
