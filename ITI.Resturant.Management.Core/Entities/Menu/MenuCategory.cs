using System.ComponentModel.DataAnnotations;

namespace ITI.Resturant.Management.Domain.Entities.Menu
{
    public class MenuCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; } // Font Awesome icon class
        public ICollection<MenuItem> MenuItems { get; set; }
    }
}