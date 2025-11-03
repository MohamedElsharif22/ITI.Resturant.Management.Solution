using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Entities.Menu
{
    public class MenuItem : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public MenuCategory Category { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int PreparationTimeInMinutes { get; set; }
        public string ImageUrl { get; set; }
        public int DailyOrderCount { get; set; } = 0;
    }

}
