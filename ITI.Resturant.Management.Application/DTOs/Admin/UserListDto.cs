using System.Collections.Generic;

namespace ITI.Resturant.Management.Application.DTOs.Admin
{
    public class UserListDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<string> Roles { get; set; } = new();
        public bool IsLockedOut { get; set; }
    }
}