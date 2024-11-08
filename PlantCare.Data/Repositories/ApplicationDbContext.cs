﻿using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<PlantDbModel> Plants { get; set; }

    public DbSet<WateringHistory> WateringHistories { get; set; }
    public DbSet<FertilizationHistory> FertilizationHistories { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseSqlite("Filename=PlantCareApp.db");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PlantDbModel>()
            .HasMany<WateringHistory>(p => p.WateringHistories)
            .WithOne(w => w.Plant)
            .HasForeignKey(w => w.PlantId)
            .OnDelete(DeleteBehavior.Cascade); // This ensures cascade delete

        modelBuilder.Entity<PlantDbModel>()
            .HasMany<FertilizationHistory>(p => p.FertilizationHistories)
            .WithOne(f => f.Plant)
            .HasForeignKey(f => f.PlantId)
            .OnDelete(DeleteBehavior.Cascade); // This ensures cascade delete
    }
}