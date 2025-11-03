using ITI.Resturant.Management.Application.DTOs;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Enums;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Order_;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ITI.Resturant.Management.MVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMenuService _menuService;

        public OrderController(IOrderService orderService, IMenuService menuService)
        {
            _orderService = orderService;
            _menuService = menuService;
        }

        // GET: Order - Requires authentication
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");

            // Admin sees all orders
            var orders = await _orderService.GetAllAsync();

            // Non-admin: filter to current user's orders only (by email)
            if (!isAdmin)
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                                ?? User.Identity?.Name ?? string.Empty;
                orders = orders.Where(o => string.Equals(o.CustomerEmail, userEmail, System.StringComparison.OrdinalIgnoreCase));
            }

            var orderSummaries = orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate,
                Total = o.Total
            }).ToList();
            return View(orderSummaries);
        }

        // GET: Order/Create - Allow anonymous to see cart, require auth to checkout
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            var items = await _menuService.GetAvailableMenuItemsAsync();
            ViewBag.MenuItems = items.ToList();

            var model = new CreateOrderDto();

            // If user is authenticated and not admin, prefill customer info from claims
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                            ?? User.Identity?.Name ?? string.Empty;

                // Try to get given_name / family_name claims, otherwise fall back to Name
                var givenName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                var familyName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
                var name = User.Identity?.Name ?? string.Empty;

                string customerName;
                if (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(familyName))
                {
                    customerName = $"{givenName ?? string.Empty} {familyName ?? string.Empty}".Trim();
                }
                else if (!string.IsNullOrWhiteSpace(name))
                {
                    customerName = name;
                }
                else
                {
                    customerName = string.Empty;
                }

                var phone = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value
                            ?? User.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value
                            ?? string.Empty;

                model.CustomerEmail = email;
                model.CustomerName = customerName;
                model.CustomerPhone = phone;
            }

            return View(model);
        }

        // POST: Order/Create - Requires authentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                var items = await _menuService.GetAvailableMenuItemsAsync();
                ViewBag.MenuItems = items.ToList();
                return View(orderDto);
            }

            var order = new Order
            {
                CustomerName = orderDto.CustomerName,
                CustomerPhone = orderDto.CustomerPhone,
                CustomerEmail = orderDto.CustomerEmail ?? string.Empty,
                OrderType = Enum.Parse<OrderType>(orderDto.OrderType),
                DeliveryAddress = orderDto.DeliveryAddress ?? string.Empty,
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                }).ToList()
            };

            try
            {
                await _orderService.CreateAsync(order);

                // Clear cart after successful order (for customers)
                if (!User.IsInRole("Admin"))
                {
                    HttpContext.Session.Remove("ShoppingCart");
                }

                TempData["SuccessMessage"] = "Order created successfully!";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var items = await _menuService.GetAvailableMenuItemsAsync();
                ViewBag.MenuItems = items.ToList();
                return View(orderDto);
            }
        }

        // GET: Order/Details/5 - Requires authentication
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // Get order with related data (OrderItems, MenuItems, Categories)
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            // No extra loading required because specification loads related data
            return View(order);
        }

        // POST: Order/Cancel/5 - Requires authentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            if (await _orderService.CanCancelOrderAsync(id))
            {
                await _orderService.CancelOrderAsync(id);
                TempData["SuccessMessage"] = "Order cancelled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "This order cannot be cancelled.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Order/UpdateStatus - Requires Admin or Staff role
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            await _orderService.UpdateStatusAsync(id, status);
            TempData["SuccessMessage"] = "Order status updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}