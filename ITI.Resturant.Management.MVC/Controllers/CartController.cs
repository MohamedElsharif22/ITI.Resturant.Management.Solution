using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Cart_;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private const string CartIdCookieName = "CartId";

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            return Json(cart ?? new Cart(cartId));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId) ?? new Cart(cartId);

            cart.AddItem(request.MenuItemId, request.Name, request.Price, request.ImageUrl ?? "", request.Category ?? "", request.Quantity);
            
            await _cartService.UpdateCartAsync(cart);
            return Json(cart);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            
            if (cart == null)
                return NotFound();

            cart.UpdateItemQuantity(request.MenuItemId, request.Quantity);
            
            await _cartService.UpdateCartAsync(cart);
            return Json(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            
            if (cart != null)
            {
                cart.Clear();
                await _cartService.UpdateCartAsync(cart);
            }

            return Json(new { success = true });
        }

        private string GetOrCreateCartId()
        {
            if (Request.Cookies.TryGetValue(CartIdCookieName, out string? existingId) && !string.IsNullOrEmpty(existingId))
                return existingId;

            var newCartId = System.Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions 
            { 
                IsEssential = true,
                Expires = System.DateTime.Now.AddYears(1)
            };

            Response.Cookies.Append(CartIdCookieName, newCartId, cookieOptions);
            return newCartId;
        }
    }

    public class AddToCartRequest
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateQuantityRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}