using System.Linq.Expressions;

namespace BookWeb.Repository.IRepository;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetByIdAsync(int id);
    Task<TEntity> GetByIdAsync(string id);

    Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task AddAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> antities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void UpdateRange(IEnumerable<TEntity> entities);

    Task UpdatePropertyAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> propertyExpression, object newValue);

    Task<int> SaveChangesAsync();
}