
using ITI.Resturant.Management.Domain.Entities;
using ITI.Resturant.Management.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain.Repositories.Contracts
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetWithSpecsAsync(ISpecification<TEntity> specs);
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllWithSpecsAsync(ISpecification<TEntity> specs);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<int> GetCountWithspecsAsync(ISpecification<TEntity> specs);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
