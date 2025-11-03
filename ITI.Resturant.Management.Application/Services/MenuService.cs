using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITI.Resturant.Management.Domain.Entities.Order_;

namespace ITI.Resturant.Management.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _lastReset = DateTime.Today;
        private static readonly SemaphoreSlim _resetSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _inventoryLock = new SemaphoreSlim(1, 1);

        public MenuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MenuItem>> GetAvailableMenuItemsAsync()
        {
            await EnsureDailyResetAsync();
            var items = await _unitOfWork.Repository<MenuItem>().GetAllAsync();
            return items.Where(i => i.IsAvailable);
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            return await _unitOfWork.Repository<MenuItem>().GetAllAsync();
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
        {
            return await _unitOfWork.Repository<MenuItem>().GetByIdAsync(id);
        }

        public async Task<int> CreateMenuItemAsync(MenuItem item)
        {
            _unitOfWork.Repository<MenuItem>().Add(item);
            await _unitOfWork.CompleteAsync();
            return item.Id;
        }

        public async Task<bool> UpdateMenuItemAsync(MenuItem item)
        {
            _unitOfWork.Repository<MenuItem>().Update(item);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var item = await GetMenuItemByIdAsync(id);
            if (item == null) return false;

            _unitOfWork.Repository<MenuItem>().Delete(item);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> ToggleAvailabilityAsync(int id)
        {
            var item = await GetMenuItemByIdAsync(id);
            if (item == null) return false;

            item.IsAvailable = !item.IsAvailable;
            _unitOfWork.Repository<MenuItem>().Update(item);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<bool> UpdateDailyCountersAsync()
        {
            var items = await _unitOfWork.Repository<MenuItem>().GetAllAsync();
            foreach (var item in items)
            {
                item.DailyOrderCount = 0;
                item.IsAvailable = true;
                _unitOfWork.Repository<MenuItem>().Update(item);
            }
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(int categoryId)
        {
            var items = await GetAllMenuItemsAsync();
            return items.Where(i => i.CategoryId == categoryId);
        }

        public async Task<bool> ValidateAndDecrementInventoryAsync(IEnumerable<OrderItem> items)
        {
            await _inventoryLock.WaitAsync();
            try
            {
                foreach (var item in items)
                {
                    var menuItem = await _unitOfWork.Repository<MenuItem>().GetByIdAsync(item.MenuItemId);
                    if (menuItem == null || !menuItem.IsAvailable)
                        return false;
                        
                    if (menuItem.DailyOrderCount + item.Quantity > 50)
                        return false;
                        
                    menuItem.DailyOrderCount += item.Quantity;
                    if (menuItem.DailyOrderCount >= 50)
                    {
                        menuItem.IsAvailable = false;
                    }
                    _unitOfWork.Repository<MenuItem>().Update(menuItem);
                }
                
                await _unitOfWork.CompleteAsync();
                return true;
            }
            finally
            {
                _inventoryLock.Release();
            }
        }

        public async Task<IEnumerable<MenuItem>> SearchAsync(string searchTerm)
        {
            var items = await GetAllMenuItemsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return items;

            searchTerm = searchTerm.ToLower();
            return items.Where(i => 
                i.Name.ToLower().Contains(searchTerm) || 
                (i.Description != null && i.Description.ToLower().Contains(searchTerm)));
        }

        public async Task<bool> CheckItemAvailabilityAsync(int itemId)
        {
            var item = await GetMenuItemByIdAsync(itemId);
            return item?.IsAvailable == true && item.DailyOrderCount < 50;
        }

        public async Task<IEnumerable<MenuCategory>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<MenuCategory>().GetAllAsync();
            var items = await GetAllMenuItemsAsync();
            
            // Only return categories that have at least one available item
            return categories.Where(c => 
                items.Any(i => i.CategoryId == c.Id && i.IsAvailable));
        }

        public async Task<bool> BulkDeleteAsync(IEnumerable<int> itemIds)
        {
            foreach (var id in itemIds)
            {
                var item = await GetMenuItemByIdAsync(id);
                if (item != null)
                {
                    _unitOfWork.Repository<MenuItem>().Delete(item);
                }
            }
            return await _unitOfWork.CompleteAsync() > 0;
        }

        private async Task EnsureDailyResetAsync()
        {
            await _resetSemaphore.WaitAsync();
            try
            {
                if (_lastReset.Date < DateTime.Today)
                {
                    await UpdateDailyCountersAsync();
                    _lastReset = DateTime.Today;
                }
            }
            finally
            {
                _resetSemaphore.Release();
            }
        }
    }
}
