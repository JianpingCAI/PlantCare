using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PlantDbModel> Plants { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Log> Logs { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseSqlite("Filename=PlantCareApp.db");
    //}
}