using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Application.DTOs.Menu
{
    /// <summary>
    /// DTO used for creating/updating menu items (Menu namespace to match mapping)
    /// </summary>
    public class MenuItemDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Preparation time must be positive")]
        public int PreparationTimeInMinutes { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Url]
        public string? ImageUrl { get; set; }
    }
}
