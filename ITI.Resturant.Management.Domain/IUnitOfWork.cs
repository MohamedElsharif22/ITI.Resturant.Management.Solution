using ITI.Resturant.Management.Domain.Entities;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Domain
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        TRepo Repository<TEntity, TRepo>()
            where TRepo : IRepository<TEntity>
            where TEntity : BaseEntity;
        Task<int> CompleteAsync();
    }
}
