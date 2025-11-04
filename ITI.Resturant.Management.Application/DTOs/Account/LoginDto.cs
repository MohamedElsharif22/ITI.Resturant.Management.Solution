using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Application.DTOs.Account
{
    public class LoginDto
    {
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Email Or user Name is requied")]
        public string EmailorUserName { get; set; }

        public string? UserName { get; set; }


        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
