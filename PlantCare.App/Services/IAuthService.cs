using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface IAuthService
{
    Task<bool> SignUpAsync(User user);

    Task<User> LoginAsync(string email, string password);
}