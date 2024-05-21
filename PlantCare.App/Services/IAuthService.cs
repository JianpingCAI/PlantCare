using PlantCare.Data.DbModels;

namespace PlantCare.App.Services;

public interface IAuthService
{
    Task<bool> SignUpAsync(User user);

    Task<User> LoginAsync(string email, string password);
}