using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public interface IUserRepository
{
    Task<User> AddAsync(User user);

    Task<User> FindByEmailAsync(string email);

    Task<bool> ExistsByEmail(string email);
}