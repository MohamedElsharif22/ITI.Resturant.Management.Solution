using ITI.Resturant.Management.Domain.Entities.Enums;
using ITI.Resturant.Management.Domain.Entities.Order_;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<int> CreateAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatusAsync(int id, OrderStatus status);
        Task<bool> CancelOrderAsync(int id);
        Task<DateTime> EstimateDeliveryTimeAsync(Order order);

        // Additional helpers
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime start, DateTime end);
        decimal CalculateOrderTotal(Order order);
        Task<bool> CanCancelOrderAsync(int orderId);
    }
}
