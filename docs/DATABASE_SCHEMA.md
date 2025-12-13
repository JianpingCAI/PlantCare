# Database Schema Documentation

## Overview

PlantCare uses SQLite as its local database, managed through Entity Framework Core with a code-first approach. The database stores plant information and care history.

**Database File**: `plants.db`  
**Location**: `FileSystem.AppDataDirectory`  
**ORM**: Entity Framework Core 10.0  
**Provider**: Microsoft.EntityFrameworkCore.Sqlite

## Database Context

**Class**: `ApplicationDbContext`  
**Location**: `PlantCare.Data/Repositories/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<PlantDbModel> Plants { get; set; }
    public DbSet<WateringHistory> WateringHistories { get; set; }
    public DbSet<FertilizationHistory> FertilizationHistories { get; set; }
}
```

## Schema Diagram

```
┌─────────────────────────┐
│       Plants            │
├─────────────────────────┤
│ Id (PK)                 │
│ Name                    │
│ Species                 │
│ Age                     │
│ PhotoPath               │
│ LastWatered             │
│ WateringFrequencyInHours│
│ LastFertilized          │
│ FertilizeFrequencyInHours│
│ Notes                   │
└────────────┬────────────┘
             │
             │ 1:N
     ┌───────┴────────┐
     │                │
┌────▼────────────┐  ┌▼─────────────────────┐
│WateringHistory  │  │FertilizationHistory  │
├─────────────────┤  ├──────────────────────┤
│ Id (PK)         │  │ Id (PK)              │
│ PlantId (FK)    │  │ PlantId (FK)         │
│ CareTime        │  │ CareTime             │
└─────────────────┘  └──────────────────────┘
```

## Table Definitions

### Plants

Stores plant information and care schedules.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | TEXT (Guid) | PRIMARY KEY | Unique plant identifier |
| Name | TEXT | NOT NULL | Plant name (max 100 chars) |
| Species | TEXT | NULL | Plant species/type |
| Age | INTEGER | NOT NULL | Plant age in years |
| PhotoPath | TEXT | NULL | Path to plant photo |
| LastWatered | TEXT (DateTime) | NOT NULL | Last watering timestamp |
| WateringFrequencyInHours | INTEGER | NOT NULL | Hours between waterings |
| LastFertilized | TEXT (DateTime) | NOT NULL | Last fertilization timestamp |
| FertilizeFrequencyInHours | INTEGER | NOT NULL | Hours between fertilizations |
| Notes | TEXT | NULL | Additional notes about the plant |

**Indexes**:
- Primary Key: `Id`
- Index on `Name` (for search optimization)

**Entity Class**: `PlantDbModel`

```csharp
public class PlantDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Species { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime LastWatered { get; set; }
    public int WateringFrequencyInHours { get; set; }
    public DateTime LastFertilized { get; set; }
    public int FertilizeFrequencyInHours { get; set; }
    public string Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public List<WateringHistory> WateringHistories { get; set; } = [];
    public List<FertilizationHistory> FertilizationHistories { get; set; } = [];
}
```

### WateringHistory

Tracks all watering events for each plant.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | TEXT (Guid) | PRIMARY KEY | Unique history record identifier |
| PlantId | TEXT (Guid) | FOREIGN KEY, NOT NULL | Reference to Plants.Id |
| CareTime | TEXT (DateTime) | NOT NULL | When the plant was watered |

**Relationships**:
- Many-to-One with `Plants` (via `PlantId`)
- Cascade delete: Deleting a plant deletes its watering history

**Indexes**:
- Primary Key: `Id`
- Foreign Key Index: `PlantId`
- Index on `CareTime` (for date queries)

**Entity Class**: `WateringHistory`

```csharp
public class WateringHistory : EventHistoryBase
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime CareTime { get; set; }
    
    // Navigation property
    public PlantDbModel Plant { get; set; } = default!;
}
```

### FertilizationHistory

Tracks all fertilization events for each plant.

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | TEXT (Guid) | PRIMARY KEY | Unique history record identifier |
| PlantId | TEXT (Guid) | FOREIGN KEY, NOT NULL | Reference to Plants.Id |
| CareTime | TEXT (DateTime) | NOT NULL | When the plant was fertilized |

**Relationships**:
- Many-to-One with `Plants` (via `PlantId`)
- Cascade delete: Deleting a plant deletes its fertilization history

**Indexes**:
- Primary Key: `Id`
- Foreign Key Index: `PlantId`
- Index on `CareTime` (for date queries)

**Entity Class**: `FertilizationHistory`

```csharp
public class FertilizationHistory : EventHistoryBase
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime CareTime { get; set; }
    
    // Navigation property
    public PlantDbModel Plant { get; set; } = default!;
}
```

## Relationships

### One-to-Many Relationships

**Plants → WateringHistory**
```csharp
modelBuilder.Entity<PlantDbModel>()
    .HasMany(p => p.WateringHistories)
    .WithOne(w => w.Plant)
    .HasForeignKey(w => w.PlantId)
    .OnDelete(DeleteBehavior.Cascade);
```

**Plants → FertilizationHistory**
```csharp
modelBuilder.Entity<PlantDbModel>()
    .HasMany(p => p.FertilizationHistories)
    .WithOne(f => f.Plant)
    .HasForeignKey(f => f.PlantId)
    .OnDelete(DeleteBehavior.Cascade);
```

## Data Constraints

### Validation Rules

**PlantDbModel**:
- `Name`: Required, max 100 characters
- `Age`: 0 to 1000 years
- `WateringFrequencyInHours`: 1 to 8760 hours (1 year)
- `FertilizeFrequencyInHours`: 1 to 8760 hours (1 year)
- `PhotoPath`: Valid file path or empty

**WateringHistory**:
- `CareTime`: Cannot be in the future
- `PlantId`: Must reference existing plant

**FertilizationHistory**:
- `CareTime`: Cannot be in the future
- `PlantId`: Must reference existing plant

## Migrations

### Migration History

| Version | Migration Name | Date | Description |
|---------|---------------|------|-------------|
| 1 | 20240521170322_InitialCreate | 2024-05-21 | Initial database schema |
| 2 | 20240528164442_AddFertilize | 2024-05-28 | Added fertilization fields |
| 3 | 20240610015602_AddWateringAndFertilizationHistoryTables | 2024-06-10 | Added history tracking tables |
| 4 | 20240612020121_RemoveUnusedTable | 2024-06-12 | Cleanup unused tables |
| 5 | 20240614022629_AddNotesColumnInPlants | 2024-06-14 | Added notes field to plants |
| 6 | 20240614034902_AddInitialRowsToWateringHistoryAndFertilizationHistory | 2024-06-14 | Seed initial history data |
| 7 | 20241029022819_remove user table | 2024-10-29 | Removed unused user table |

### Creating New Migration

```bash
cd PlantCare.Data

# Create migration
dotnet ef migrations add MigrationName --startup-project ../PlantCare.App

# Apply migration
dotnet ef database update --startup-project ../PlantCare.App
```

### Rolling Back Migration

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName --startup-project ../PlantCare.App

# Remove last migration (before applying)
dotnet ef migrations remove --startup-project ../PlantCare.App
```

## Queries

### Common Query Patterns

**Get All Plants with History**:
```csharp
await _context.Plants
    .Include(p => p.WateringHistories)
    .Include(p => p.FertilizationHistories)
    .OrderBy(p => p.Name)
    .ToListAsync();
```

**Get Plant by ID**:
```csharp
await _context.Plants
    .FirstOrDefaultAsync(p => p.Id == plantId);
```

**Get Watering History for Plant**:
```csharp
await _context.WateringHistories
    .Where(w => w.PlantId == plantId)
    .OrderBy(w => w.CareTime)
    .ToListAsync();
```

**Get Plants Needing Water**:
```csharp
var now = DateTime.Now;
var plantsNeedingWater = await _context.Plants
    .Where(p => p.LastWatered.AddHours(p.WateringFrequencyInHours) <= now)
    .ToListAsync();
```

## Performance Considerations

### Indexes

The following indexes are automatically created by EF Core:
- Primary keys on all tables
- Foreign key indexes on `PlantId` in history tables

### Optimization Tips

1. **Use Projections**: Select only needed fields
   ```csharp
   .Select(p => new { p.Id, p.Name, p.LastWatered })
   ```

2. **Use AsNoTracking**: For read-only queries
   ```csharp
   .AsNoTracking().ToListAsync()
   ```

3. **Avoid N+1 Queries**: Use `.Include()` for related data
   ```csharp
   .Include(p => p.WateringHistories)
   ```

4. **Batch Operations**: Use `AddRange()` instead of multiple `Add()` calls
   ```csharp
   context.Plants.AddRange(plants);
   ```

## Database Seeding

No default seed data is provided. The database starts empty and is populated by user actions.

### Initial Data (Example)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Example: Seed demo plant (not used in production)
    modelBuilder.Entity<PlantDbModel>().HasData(
        new PlantDbModel
        {
            Id = Guid.NewGuid(),
            Name = "Demo Plant",
            Species = "Example Species",
            Age = 1,
            PhotoPath = "/default.png",
            LastWatered = DateTime.Now,
            WateringFrequencyInHours = 72,
            LastFertilized = DateTime.Now,
            FertilizeFrequencyInHours = 720
        }
    );
}
```

## Backup and Restore

### Manual Backup

```bash
# Android
adb pull /data/data/com.jianping.myplantcare.app/files/plants.db ./backup/

# iOS Simulator
cp ~/Library/Developer/CoreSimulator/Devices/{DEVICE-ID}/data/Containers/Data/Application/{APP-ID}/Library/plants.db ./backup/
```

### Restore Database

```bash
# Android
adb push ./backup/plants.db /data/data/com.jianping.myplantcare.app/files/

# iOS Simulator
cp ./backup/plants.db ~/Library/Developer/CoreSimulator/Devices/{DEVICE-ID}/data/Containers/Data/Application/{APP-ID}/Library/
```

### Programmatic Backup (via Export Feature)

Users can export all data including the database through the app's export functionality:
1. Go to Settings
2. Tap "Export Data"
3. Select destination folder
4. Data exported as ZIP file including database and photos

## Database Maintenance

### Clear All Data

**Via Service**:
```csharp
await _plantService.ClearAllTablesAsync();
await _plantService.DeleteAllPhotosAsync();
```

**Via SQL**:
```sql
DELETE FROM FertilizationHistories;
DELETE FROM WateringHistories;
DELETE FROM Plants;
VACUUM; -- Reclaim space
```

### Vacuum Database

Periodically optimize database size:

```csharp
await _context.Database.ExecuteSqlRawAsync("VACUUM");
```

## Troubleshooting

### Migration Errors

**Error**: "A migration has already been applied"

**Solution**:
```bash
dotnet ef database drop --startup-project ../PlantCare.App
dotnet ef database update --startup-project ../PlantCare.App
```

### Database Locked

**Error**: "database is locked"

**Causes**:
- Multiple connections to database
- Long-running transaction
- File permissions

**Solution**:
- Ensure DbContext is properly disposed
- Use `using` statements for DbContext
- Check for background tasks accessing database

### Corrupt Database

**Solution**:
1. Export user data (if possible)
2. Delete database file
3. Restart app (database will be recreated)
4. Import data back

## Schema Changes Planning

When planning schema changes:

1. **Assess Impact**: Check all code using affected entities
2. **Create Migration**: Generate EF Core migration
3. **Test Migration**: Test on development database first
4. **Data Migration**: Plan how to migrate existing data
5. **Rollback Plan**: Prepare rollback migration if needed
6. **Update Models**: Update all affected models and DTOs
7. **Update Queries**: Update repository queries
8. **Test**: Run all tests to ensure nothing breaks

---

**Last Updated**: 2024-XX-XX  
**Schema Version**: 7 (Migration: 20241029022819_remove user table)
