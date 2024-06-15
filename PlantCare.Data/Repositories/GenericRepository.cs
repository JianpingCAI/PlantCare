using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        T? trackedEntity = await FindTrackedEntity(entity);

        if (trackedEntity == null)
        {
            // Entity is not found in the database, either it doesn't exist or there's a data issue
            return false;
        }
        else
        {
            // Entity is found, update its values
            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is PlantDbModel)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        // The entity has been deleted
                        throw new Exception("The entity has been deleted by another user.");
                    }
                    else
                    {
                        // Update the original values with the database values
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
                else
                {
                    throw new NotSupportedException("Concurrency conflict detected for an unknown entity type.");
                }
            }

            // Retry the update operation
            await _context.SaveChangesAsync();
        }

        return true;
    }

    private async Task<T?> FindTrackedEntity(T entity)
    {
        // Get the primary key value(s)
        IReadOnlyList<IProperty>? keyProperties = _context.Entry(entity).Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties is null)
        {
            return null;
        }
        object?[]? keyValues = keyProperties.Select(p => _context.Entry(entity).Property(p.Name).CurrentValue).ToArray();
        if (keyValues is null || keyValues.Any(k => k == null))
        {
            return null;
        }

        // Attempt to find the entity in the database
        /*
         - If there is an entity in the database with the same primary key, DbSet.FindAsync should return that entity.
         - If the context is not currently tracking an entity with that primary key, DbSet.FindAsync will query the database to find it.
         - If the entity is found in the database, it will be returned and tracked by the context.
         - If the entity is not found in the database, DbSet.FindAsync will return null.
         */
        return await _dbSet.FindAsync(keyValues.Cast<object>().ToArray());
    }

    public async Task<T?> DeleteAsync(Guid itemId)
    {
        T? entity = await _dbSet.FindAsync(itemId);

        if (entity != null)
        {
            //_dbSet.Entry(entity).State = EntityState.Detached;
            _dbSet.Remove(entity);
            int count = await _context.SaveChangesAsync();

            if (count == 1)
            {
                return entity;
            }
        }

        return entity;
    }

    //public async Task DeleteAsync(T entity)
    //{
    //    // Check if the entity is already tracked by the context
    //    var trackedEntity = await _dbSet.FindAsync(_context.Entry(entity).Property("Id").CurrentValue);

    //    if (trackedEntity == null)
    //    {
    //        // Entity is not tracked, attach it to the context
    //        _dbSet.Attach(entity);
    //    }

    //    _dbSet.Remove(entity);
    //    await _context.SaveChangesAsync();
    //}
}