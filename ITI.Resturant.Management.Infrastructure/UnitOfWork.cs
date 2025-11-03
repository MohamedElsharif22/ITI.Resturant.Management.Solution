using ITI.Resturant.Management.Domain;
using ITI.Resturant.Management.Domain.Entities;
using ITI.Resturant.Management.Domain.Repositories.Contracts;
using ITI.Resturant.Management.Infrastructure._Data;
using ITI.Resturant.Management.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure
{
    public class UnitOfWork(AppDbContext context, IServiceProvider serviceProvider) : IUnitOfWork
    {
        private readonly AppDbContext _context = context;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(IRepository<TEntity>);

            return (IRepository<TEntity>)_repositories.GetOrAdd(
                type,
                _ => new Repository<TEntity>(_context)
            );
        }

        public TRepo Repository<TEntity, TRepo>()
            where TRepo : IRepository<TEntity>
            where TEntity : BaseEntity
        {
            var type = typeof(TRepo);

            return (TRepo) _repositories.GetOrAdd(
                type,
                _ => _serviceProvider.GetRequiredService<TRepo>()
            );
        }

        public async Task<int> CompleteAsync()
            => await _context.SaveChangesAsync();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();
    }
}
