using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "ShoppingCart";

        // Get cart from session
        [HttpGet]
        public IActionResult GetCart()
        {
            var cart = GetCartFromSession();
            return Json(cart);
        }

        // Add item to cart
        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItemDto item)
        {
            var cart = GetCartFromSession();

            var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(item);
            }

            SaveCartToSession(cart);
            return Json(cart);
        }

        // Update item quantity
        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityDto dto)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == dto.MenuItemId);

            if (item != null)
            {
                if (dto.Quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = dto.Quantity;
                }
            }

            SaveCartToSession(cart);
            return Json(cart);
        }

        // Clear cart
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return Json(new CartDto { Items = new List<CartItemDto>() });
        }

        private CartDto GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartDto { Items = new List<CartItemDto>() };
            }

            return JsonSerializer.Deserialize<CartDto>(cartJson) ?? new CartDto { Items = new List<CartItemDto>() };
        }

        private void SaveCartToSession(CartDto cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }
    }

    // DTOs
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQuantityDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}