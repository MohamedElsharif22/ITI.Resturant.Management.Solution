using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _lastReset = DateTime.Today;
        private static readonly SemaphoreSlim _resetSemaphore = new SemaphoreSlim(1, 1);

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
            return await _unitOfWork.Repository<MenuItem, IMenuRepository>().GetByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<MenuItem>> SearchAsync(string term)
        {
            return await _unitOfWork.Repository<MenuItem, IMenuRepository>().SearchAsync(term);
        }

        public async Task<bool> CheckItemAvailabilityAsync(int itemId)
        {
            await EnsureDailyResetAsync();
            var item = await GetMenuItemByIdAsync(itemId);
            if (item == null) return false;
            return item.IsAvailable && item.DailyOrderCount < 50;
        }

        public async Task<IEnumerable<MenuCategory>> GetActiveCategoriesAsync()
        {
            await EnsureDailyResetAsync();
            var categories = await _unitOfWork.Repository<MenuCategory>().GetAllAsync();
            var items = await _unitOfWork.Repository<MenuItem>().GetAllAsync();
            return categories.Where(c => items.Any(i => i.CategoryId == c.Id && i.IsAvailable));
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            if (ids == null) return;
            foreach (var id in ids)
            {
                var entity = await GetMenuItemByIdAsync(id);
                if (entity == null) continue;
                _unitOfWork.Repository<MenuItem>().Delete(entity);
            }
            await _unitOfWork.CompleteAsync();
        }

        private async Task EnsureDailyResetAsync()
        {
            var today = DateTime.Today;
            if (_lastReset >= today) return;

            await _resetSemaphore.WaitAsync();
            try
            {
                if (_lastReset >= today) return;

                var repo = _unitOfWork.Repository<MenuItem>();
                var items = (await repo.GetAllAsync()).ToList();

                foreach (var it in items)
                {
                    it.DailyOrderCount = 0;
                    it.IsAvailable = true;
                    repo.Update(it);
                }

                await _unitOfWork.CompleteAsync();
                _lastReset = today;
            }
            finally
            {
                _resetSemaphore.Release();
            }
        }
    }
}
