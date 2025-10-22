using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskPomodoro.Api.Data;

namespace TaskPomodoro.Api.Data;

/// <summary>
/// A repository implementation using Entity Framework Core
/// </summary>
/// <typeparam name="T">The type of the entity</typeparam>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">DB context</param>
    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Get an entity by its id
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <returns>The entity or null if not found</returns>
    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    /// <summary>
    /// Get all entities    
    /// </summary>
    /// <returns>All entities</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    /// <summary>
    /// Find entities by a predicate
    /// </summary>
    /// <param name="predicate">The predicate</param>
    /// <returns>The entities</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();

    /// <summary>
    /// Add an entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <returns>The added entity</returns>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity">The entity</param>
    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity">The entity</param>
    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get a queryable of all entities
    /// </summary>
    /// <returns>A queryable of all entities</returns>
    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();
}