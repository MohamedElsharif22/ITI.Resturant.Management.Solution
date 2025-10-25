namespace ITI.Resturant.Management.Domain.Entities.Cart_
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

    }
}