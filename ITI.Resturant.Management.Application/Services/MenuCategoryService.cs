using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain;
using ITI.Resturant.Management.Domain.Repositories.Contracts;

namespace ITI.Resturant.Management.Application.Services
{
    public interface IMenuCategoryService
    {
        Task<IEnumerable<MenuCategory>> GetActiveCategories();
        Task<MenuCategory?> GetByIdAsync(int id);
        Task<IEnumerable<MenuCategory>> GetAllAsync();
        Task AddAsync(MenuCategory category);
        Task UpdateAsync(MenuCategory category);
        Task DeleteAsync(int id);
    }

    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MenuCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MenuCategory>> GetActiveCategories()
        {
            var categories = await _unitOfWork.Repository<MenuCategory>().GetAllAsync();
            var result = new List<MenuCategory>();

            foreach (var category in categories)
            {
                // Use generic repository to fetch menu items and filter by category
                var allItems = await _unitOfWork.Repository<MenuItem>().GetAllAsync();
                var items = allItems.Where(mi => mi.CategoryId == category.Id);

                // Only include categories that have at least one available item
                if (items.Any(i => i.IsAvailable))
                {
                    result.Add(category);
                }
            }

            return result;
        }

        public async Task<MenuCategory?> GetByIdAsync(int id)
            => await _unitOfWork.Repository<MenuCategory>().GetByIdAsync(id);

        public async Task<IEnumerable<MenuCategory>> GetAllAsync()
            => await _unitOfWork.Repository<MenuCategory>().GetAllAsync();

        public async Task AddAsync(MenuCategory category)
        {
            _unitOfWork.Repository<MenuCategory>().Add(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(MenuCategory category)
        {
            _unitOfWork.Repository<MenuCategory>().Update(category);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return;
            _unitOfWork.Repository<MenuCategory>().Delete(entity);
            await _unitOfWork.CompleteAsync();
        }
    }
}