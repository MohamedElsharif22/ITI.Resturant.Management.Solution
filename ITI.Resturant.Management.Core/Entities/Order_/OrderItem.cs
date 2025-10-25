using ITI.Resturant.Management.Domain.Entities.Menu;
using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Domain.Entities.Order_
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}