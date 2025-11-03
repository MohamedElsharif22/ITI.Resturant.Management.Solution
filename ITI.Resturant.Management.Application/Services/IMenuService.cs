using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Order_;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuItem>> GetAvailableMenuItemsAsync();
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<MenuItem?> GetMenuItemByIdAsync(int id);
        Task<int> CreateMenuItemAsync(MenuItem item);
        Task<bool> UpdateMenuItemAsync(MenuItem item);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<bool> ToggleAvailabilityAsync(int id);
        Task<bool> UpdateDailyCountersAsync();
        Task<bool> ValidateAndDecrementInventoryAsync(IEnumerable<OrderItem> items);
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<MenuItem>> SearchAsync(string searchTerm);
        Task<bool> CheckItemAvailabilityAsync(int itemId);
        Task<IEnumerable<MenuCategory>> GetActiveCategoriesAsync();
        Task<bool> BulkDeleteAsync(IEnumerable<int> itemIds);
    }
}
