using ITI.Resturant.Management.Domain.Entities.Menu;

namespace ITI.Resturant.Management.MVC.Models
{
    public class HomeViewModel
    {
        public List<MenuItem> FeaturedMenuItems { get; set; } = new();
        public List<MenuCategory> Categories { get; set; } = new();
    }
}