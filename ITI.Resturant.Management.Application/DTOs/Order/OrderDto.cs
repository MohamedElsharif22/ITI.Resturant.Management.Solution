using System;
using System.Collections.Generic;

namespace ITI.Resturant.Management.Application.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string OrderType { get; set; } = "DineIn";
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public List<OrderItemDto>? Items { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
