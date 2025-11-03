using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Application.DTOs.Admin
{
    public class MenuItemAdminDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int PreparationTimeInMinutes { get; set; }

        public bool IsAvailable { get; set; }

        public string? ImageUrl { get; set; }
    }
}
