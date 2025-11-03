using ITI.Resturant.Management.Application.Services;
using ITI.Resturant.Management.Domain.Entities.Order_;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Enums;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var items = await _menuService.GetAvailableMenuItemsAsync();
            ViewData["Items"] = items;
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Items"] = await _menuService.GetAvailableMenuItemsAsync();
                return View(order);
            }

            if (!order.ValidateOrder())
            {
                ModelState.AddModelError(string.Empty, "Invalid order data");
                ViewData["Items"] = await _menuService.GetAvailableMenuItemsAsync();
                return View(order);
            }

            try
            {
                var orderId = await _orderService.CreateAsync(order);
                TempData["SuccessMessage"] = "Order placed successfully";
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewData["Items"] = await _menuService.GetAvailableMenuItemsAsync();
                return View(order);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int orderId, int menuItemId, int quantity)
        {
            if (quantity <= 0) return BadRequest("Quantity must be positive");
            try
            {
                await _orderService.CreateAsync(new Order
                {
                    // simplistic: create a new order item via service API would be better
                });
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                TempData["SuccessMessage"] = "Order cancelled";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Details", new { id });
        }
    }
}
