using Microsoft.EntityFrameworkCore;
using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Log> Logs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=PlantCareApp.db");
    }
}