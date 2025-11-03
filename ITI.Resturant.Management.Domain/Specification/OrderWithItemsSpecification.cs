using ITI.Resturant.Management.Domain.Entities.Order_;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace ITI.Resturant.Management.Domain.Specification
{
    public class OrderWithItemsSpecification : BaseSpecification<Order>
    {
        public OrderWithItemsSpecification(int id) : base(o => o.Id == id)
        {
            // Include OrderItems and their MenuItem navigation
            AddInclude(q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).ThenInclude(oi => oi.Category));
        }
    }
}
