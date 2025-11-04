using System.Collections.Generic;

namespace ITI.Resturant.Management.Application.DTOs.Admin
{
    public class UserDetailsDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}