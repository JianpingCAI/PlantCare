namespace PlantCare.Data.Repositories.interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();

    Task<T?> GetByIdAsync(Guid id);

    Task<T> AddAsync(T entity);

    Task<bool> UpdateAsync(T entity);

    //Task DeleteAsync(T entity);
    Task<T> DeleteAsync(Guid itemId);
}