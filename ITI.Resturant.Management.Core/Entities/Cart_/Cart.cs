using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Entities.Cart_
{
    public class Cart(string id)
    {
        public string Id { get; set; } = id;
        public const decimal Tax = 0.14m;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalPriceBeforeTax => Items.Sum(item => item.Price);
        public decimal TotalTax => TotalPriceBeforeTax * Tax;
        public decimal TotalPrice => TotalPriceBeforeTax + TotalTax;
    }
}