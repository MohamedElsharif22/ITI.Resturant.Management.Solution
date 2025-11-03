using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Infrastructure._Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Repositories
{
    public class MenuRepository : Repository<MenuItem>, IMenuRepository
    {
        public MenuRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<MenuItem>> GetByCategoryAsync(int categoryId)
        {
            return await _Context.MenuItems.Where(m => m.CategoryId == categoryId && !m.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<MenuItem>> SearchAsync(string term)
        {
            return await _Context.MenuItems
                .Where(m => !m.IsDeleted && (m.Name.Contains(term) || m.Description.Contains(term)))
                .ToListAsync();
        }
    }
}
