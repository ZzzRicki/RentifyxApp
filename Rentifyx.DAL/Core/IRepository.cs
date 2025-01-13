using System.Linq.Expressions;

namespace Rentifyx.DAL.Core
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task SaveAsync(TEntity entity);
        Task SaveAsync(TEntity[] entities);
        Task UpdateAsync(TEntity entity);
        Task UpdateAsync(TEntity[] entities);
        Task RemoveAsync(TEntity entity);
        Task RemoveAsync(TEntity[] entities);
        Task<TEntity?> GetEntityAsync(int id, string? includeProperties = null);
        Task<List<TEntity>> GetEntitiesAsync(string? includeProperties = null);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null);
    }
}

