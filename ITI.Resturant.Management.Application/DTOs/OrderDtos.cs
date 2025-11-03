using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace ITI.Resturant.Management.Application.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        public string? MenuItemName { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        [Required]
        public string OrderType { get; set; } = "DineIn"; // DineIn/Takeout/Delivery

        public string? DeliveryAddress { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
    }

    public class AnalyticsDto
    {
        public decimal DailySales { get; set; }
        public IDictionary<int, int> TopItems { get; set; } = new Dictionary<int, int>();
        public IDictionary<string, int> StatusBreakdown { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, decimal> RevenueByType { get; set; } = new Dictionary<string, decimal>();
    }
}
