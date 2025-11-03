using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Repositories.Contracts
{
    public interface IMenuRepository : IRepository<MenuItem>
    {
        Task<IEnumerable<MenuItem>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<MenuItem>> SearchAsync(string term);
    }
}
