using ITI.Resturant.Management.Domain.Entities.Order_;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Enums;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountService _discountService;
        private readonly IPricingService _pricingService;
        private readonly decimal _taxRate = 0.085m; // 8.5% tax rate

        public OrderService(IUnitOfWork unitOfWork, IDiscountService discountService, IPricingService pricingService)
        {
            _unitOfWork = unitOfWork;
            _discountService = discountService;
            _pricingService = pricingService;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
            => await _unitOfWork.Repository<Order>().GetAllAsync();

        public async Task<Order?> GetByIdAsync(int id)
            => await _unitOfWork.Repository<Order>().GetByIdAsync(id);

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            var orders = await GetAllAsync();
            return orders.Where(o => o.Status == status);
        }

        public async Task<int> CreateAsync(Order order)
        {
            // validate items
            foreach (var oi in order.OrderItems)
            {
                var menu = await _unitOfWork.Repository<MenuItem>().GetByIdAsync(oi.MenuItemId);
                if (menu == null || !menu.IsAvailable)
                    throw new InvalidOperationException($"Menu item {oi.MenuItemId} is not available");

                // increment daily count
                menu.DailyOrderCount++;
                if (menu.DailyOrderCount >= 50)
                {
                    menu.IsAvailable = false;
                }
                _unitOfWork.Repository<MenuItem>().Update(menu);
            }

            // calculate totals
            order.Subtotal = order.OrderItems.Sum(x => x.Subtotal);
            order.Tax = _pricingService.CalculateTax(order.Subtotal);
            order.Discount = _discountService.CalculateDiscount(order);
            order.Total = order.Subtotal + order.Tax - order.Discount;

            if (order.OrderType == OrderType.Delivery && string.IsNullOrEmpty(order.DeliveryAddress))
                throw new InvalidOperationException("Delivery address required for delivery orders");

            order.Status = OrderStatus.Pending;
            order.EstimatedDeliveryTime = await EstimateDeliveryTimeAsync(order);

            _unitOfWork.Repository<Order>().Add(order);
            await _unitOfWork.CompleteAsync();

            // start background progression (fire-and-forget)
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(2));
                await UpdateStatusAsync(order.Id, OrderStatus.Preparing);

                var maxPrep = order.OrderItems.Select(i => (
                    _unitOfWork.Repository<MenuItem>().GetByIdAsync(i.MenuItemId).Result?.PreparationTimeInMinutes ?? 0)).Max();

                await Task.Delay(TimeSpan.FromMinutes(maxPrep));
                await UpdateStatusAsync(order.Id, OrderStatus.Ready);
            });

            return order.Id;
        }

        public async Task<bool> UpdateAsync(Order order)
        {
            var existing = await GetByIdAsync(order.Id);
            if (existing == null) return false;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;
            _unitOfWork.Repository<Order>().Delete(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, OrderStatus status)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;
            order.Status = status;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;
            if (order.Status == OrderStatus.Ready || order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel Ready or Delivered orders");
            order.Status = OrderStatus.Cancelled;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<DateTime> EstimateDeliveryTimeAsync(Order order)
        {
            var now = DateTime.Now;
            var maxPrep = order.OrderItems.Select(i => (
                _unitOfWork.Repository<MenuItem>().GetByIdAsync(i.MenuItemId).Result?.PreparationTimeInMinutes ?? 0)).Max();
            var estimated = now.AddMinutes(maxPrep);
            if (order.OrderType == OrderType.Delivery)
                estimated = estimated.AddMinutes(30);
            return estimated;
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime start, DateTime end)
        {
            var all = await GetAllAsync();
            return all.Where(o => o.OrderDate >= start && o.OrderDate <= end);
        }

        public decimal CalculateOrderTotal(Order order)
        {
            if (order == null) return 0m;
            var subtotal = order.OrderItems.Sum(x => x.Subtotal);
            var discount = _discountService.CalculateDiscount(order);
            var tax = _pricingService.CalculateTax(subtotal - discount);
            return subtotal - discount + tax;
        }

        public async Task<bool> CanCancelOrderAsync(int orderId)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return false;
            return order.Status != OrderStatus.Ready && order.Status != OrderStatus.Delivered;
        }
    }
}
