using ITI.Resturant.Management.Domain.Entities;
using ITI.Resturant.Management.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure.Specification
{
    internal class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> BuildQuery(IQueryable<TEntity> inpuQuery, ISpecification<TEntity> specs)
        {
            var query = inpuQuery;

            //Filtering
            if (specs.Criteria is not null)
                query = inpuQuery.Where(specs.Criteria);

            // Includes with ThenInclude support
            query = specs.IncludeFuncs.Aggregate(query, (current, includeFunc) => includeFunc(current));

            // Simple Includes (backward compatibility)
            query = specs.Includes.Aggregate(query, (currentQuery, currentExpression) => currentQuery.Include(currentExpression));

            // ordering
            if (specs.OrderByAsc is not null)
                query = query.OrderBy(specs.OrderByAsc);
            else if (specs.OrderByDesc is not null)
                query = query.OrderByDescending(specs.OrderByDesc);

            // Pagination
            if (specs.IsPagenationEnabled)
                query = query.Skip(specs.Skip).Take(specs.Take);

            return query;
        }
    }
}
