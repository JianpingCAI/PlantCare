using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class UserRepository(ApplicationDbContext context) : GenericRepository<User>(context), IUserRepository
{

    // Additional user-specific methods can be added here
    public async Task<bool> ExistsByEmail(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> FindByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}