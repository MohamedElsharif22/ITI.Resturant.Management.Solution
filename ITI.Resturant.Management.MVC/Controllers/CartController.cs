using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Cart_;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class CartController : Controller
    {
        private const string CartCookieName = "CartId";
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetOrCreateCartId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.Identity.Name!;
            }

            if (Request.Cookies.TryGetValue(CartCookieName, out var id) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            var newId = Guid.NewGuid().ToString();
            Response.Cookies.Append(CartCookieName, newId, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            });
            return newId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId) ?? new Cart(cartId);

            var dto = new
            {
                id = cart.Id,
                items = cart.Items.Select(i => new
                {
                    menuItemId = i.MenuItemId,
                    name = i.Name,
                    price = i.Price,
                    imageUrl = i.ImageUrl,
                    category = i.Category,
                    quantity = i.Quantity
                }),
                total = cart.Total
            };

            return Json(dto);
        }

        public record AddCartItemDto(int menuItemId, string name, decimal price, string? imageUrl, string? category, int quantity);

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddCartItemDto dto)
        {
            if (dto == null || dto.quantity <= 0) return BadRequest("Invalid item");

            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId) ?? new Cart(cartId);
            cart.AddItem(dto.menuItemId, dto.name, dto.price, dto.imageUrl ?? string.Empty, dto.category ?? string.Empty, dto.quantity);
            await _cartService.UpdateCartAsync(cart);

            var response = new
            {
                id = cart.Id,
                items = cart.Items.Select(i => new { menuItemId = i.MenuItemId, name = i.Name, price = i.Price, quantity = i.Quantity }),
                total = cart.Total
            };
            return Json(response);
        }

        public record UpdateQuantityDto(int menuItemId, int quantity);

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityDto dto)
        {
            if (dto == null) return BadRequest();

            var cartId = GetOrCreateCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            if (cart == null) return NotFound();

            cart.UpdateItemQuantity(dto.menuItemId, dto.quantity);
            await _cartService.UpdateCartAsync(cart);

            return Json(new { success = true, total = cart.Total });
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
    }
}
