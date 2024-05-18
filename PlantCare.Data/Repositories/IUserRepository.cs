using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);

    Task<User> FindByEmailAsync(string email);

    Task<bool> ExistsByEmail(string email);
}