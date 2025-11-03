using ITI.Resturant.Management.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Entities.Order_
{
    public class Order : BaseEntity
    {
        public Order()
        {
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerEmail = string.Empty;
            DeliveryAddress = string.Empty;
            SpecialInstructions = string.Empty;
            OrderItems = new List<OrderItem>();
        }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Phone]
        [Required(ErrorMessage = "Customer phone is required")]
        public string CustomerPhone { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Customer email is required")]
        public string CustomerEmail { get; set; }

        public OrderType OrderType { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue, ErrorMessage = "Subtotal must be non-negative")]
        public decimal Subtotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax must be non-negative")]
        public decimal Tax { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount must be non-negative")]
        public decimal Discount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total must be non-negative")]
        public decimal Total { get; set; }

        [RequiredIf("OrderType", OrderType.Delivery, 
            ErrorMessage = "Delivery address is required for delivery orders")]
        public string DeliveryAddress { get; set; }

        public DateTime? EstimatedDeliveryTime { get; set; }

        [StringLength(500)]
        public string SpecialInstructions { get; set; }

        public CustomerDiscountType CustomerDiscountType { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        // Helper method to validate order
        public bool ValidateOrder()
        {
            if (OrderType == OrderType.Delivery && string.IsNullOrEmpty(DeliveryAddress))
                return false;

            if (OrderItems?.Any() != true)
                return false;

            // Validate that all items are available
            if (OrderItems != null)
            {
                foreach (var item in OrderItems)
                {
                    if (item.Quantity <= 0 || item.UnitPrice < 0)
                        return false;
                }
            }

            return true;
        }
    }

    // Custom validation attribute for conditional required fields
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
                return ValidationResult.Success;

            var property = validationContext.ObjectInstance.GetType().GetProperty(_propertyName);
            if (property == null)
                return ValidationResult.Success;

            var propertyValue = property.GetValue(validationContext.ObjectInstance, null);
            
            if (propertyValue?.Equals(_desiredValue) == true && 
                (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
