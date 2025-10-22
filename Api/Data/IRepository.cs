using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TaskPomodoro.Api.Data;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get an entity by its id
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <returns>The entity</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>All entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Find entities by a predicate
    /// </summary>
    /// <param name="predicate">The predicate</param>
    /// <returns>The entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity">The entity</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity">The entity</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Get a queryable of all entities
    /// </summary>
    /// <returns>A queryable of all entities</returns>
    IQueryable<T> Query();
}