using Microsoft.EntityFrameworkCore;
using Rentifyx.DAL.Context;
using System.Linq.Expressions;


namespace Rentifyx.DAL.Core
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly RentifyxContext context;
        private DbSet<TEntity> entity;

        public Repository(RentifyxContext context)
        {
            this.context = context;
            this.entity = context.Set<TEntity>();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null)
        {
            return await this.entity.AnyAsync(filter);
        }

        public virtual Task<List<TEntity>> GetEntitiesAsync(string? includeProperties = null)
        {
            IQueryable<TEntity> query = entity;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToListAsync();
        }

        public virtual async Task<TEntity?> GetEntityAsync(int id, string? includeProperties = null)
        {
            return await this.entity.FindAsync(id);

        }

        public virtual async Task RemoveAsync(TEntity entity)
        {
            this.entity.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task RemoveAsync(TEntity[] entities)
        {
            this.entity.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task SaveAsync(TEntity entity)
        {
            await this.entity.AddAsync(entity);
        }

        public virtual async Task SaveAsync(TEntity[] entities)
        {
            await this.entity.AddRangeAsync(entities);
        }
        public virtual async Task UpdateAsync(TEntity entity)
        {
            this.context.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task UpdateAsync(TEntity[] entities)
        {
            this.context.Update(entities);
            await Task.CompletedTask;
        }
    }
}

