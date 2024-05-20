using PlantCare.Data.Models;
using PlantCare.Data.Repositories;

//using BCrypt.Net;

namespace PlantCare.App.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> SignUpAsync(User user)
    {
        if (await _userRepository.ExistsByEmail(user.Email))
        {
            throw new Exception("User already exists with the given email.");
        }

        // Hash the password before saving to database
        user.PasswordHash = HashPassword(user.PasswordHash);
        await _userRepository.AddAsync(user);
        return true;
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials.");
        }
        return user;
    }

    private string HashPassword(string password)
    {
        // Implement password hashing
        return string.Empty;
        //return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return true;
        //return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}