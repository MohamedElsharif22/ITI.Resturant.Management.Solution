using ITI.Resturant.Management.Application.DTOs;
using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Enums;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Order_;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
            var orders = await _orderService.GetAllAsync();
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
            return View(new CreateOrderDto());
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
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

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
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}