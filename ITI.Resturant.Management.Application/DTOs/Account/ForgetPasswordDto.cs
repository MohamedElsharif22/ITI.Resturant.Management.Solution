using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Application.DTOs.Account
{
    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}