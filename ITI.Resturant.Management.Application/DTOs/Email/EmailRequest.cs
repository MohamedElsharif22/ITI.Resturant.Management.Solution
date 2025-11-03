using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Application.DTOs.Email
{
    /// <summary>
    /// DTO representing an email to send via infrastructure EmailService
    /// </summary>
    public class EmailRequest
    {
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; } = string.Empty;

        public string? ToName { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;

        public string? CcEmail { get; set; }

        public string? BccEmail { get; set; }
    }
}
