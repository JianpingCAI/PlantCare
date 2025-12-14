# EF Notes

`C:\Users\cai-j\AppData\Local\Packages\com.companyname.plantcare.app_9zz4h110yvjzm\LocalState`
`/data/user/0/com.jianping.myapp/files/app.log`

## DB Migration

### Quick Reference

```cmd
# Add a new migration
Add-Migration xxx -Project PlantCare.Data -StartupProject PlantCare.Data

# Example: Add migration for removing user table
Add-Migration "remove user table" -Project PlantCare.Data -StartupProject PlantCare.Data
```

**Note**: No need to run `Update-Database` command for MAUI apps since the database is created/updated automatically at runtime.

```cmd
# This is NOT needed for MAUI apps (used only for local database files during development)
Update-Database -Project PlantCare.Data -StartupProject PlantCare.Data
```

### Database Locations

**Windows (UWP/WinUI)**:

```
C:\Users\cai-j\AppData\Local\Packages\com.companyname.plantcare.app_9zz4h110yvjzm\LocalState\
```

**Android Emulator/Device**:

```
/data/user/0/com.companyname.plantcare.app/files/PlantCareApp.db
/data/user/0/com.jianping.myplantcare.app/files/plants.db
```

---

## Entity Framework Core Migrations - Complete Tutorial

### 🎓 What are Migrations?

**Migrations** are a way to keep your database schema in sync with your EF Core model while preserving existing data.

**Key Concepts**:

- **Code-First**: Define models in C#, generate database from code
- **Migration File**: Contains instructions to update database schema
- **Snapshot**: Current state of your model in code form
- **Up/Down Methods**: Apply or revert changes

---

### 📚 Migration Basics

#### 1. Initial Migration (Creating Database)

When you start a new project:

```bash
# Using Package Manager Console (Visual Studio)
Add-Migration InitialCreate -Project PlantCare.Data -StartupProject PlantCare.Data

# Using .NET CLI
dotnet ef migrations add InitialCreate --project PlantCare.Data --startup-project PlantCare.Data
```

**What this does**:

- Creates `Migrations/` folder in PlantCare.Data
- Generates 3 files:
  - `{timestamp}_InitialCreate.cs` - Migration class with Up() and Down() methods
  - `{timestamp}_InitialCreate.Designer.cs` - Migration metadata
  - `ApplicationDbContextModelSnapshot.cs` - Current model snapshot

**Example Initial Migration**:

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Plants",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Species = table.Column<string>(nullable: true),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Plants", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Plants");
    }
}
```

#### 2. Adding a New Column

**Step 1**: Update your model

```csharp
public class PlantDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // NEW: Add notes field
    public string Notes { get; set; } = string.Empty;
}
```

**Step 2**: Create migration

```bash
Add-Migration AddNotesColumnInPlants -Project PlantCare.Data
```

**Generated Migration**:

```csharp
public partial class AddNotesColumnInPlants : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "Plants",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Notes",
            table: "Plants");
    }
}
```

#### 3. Adding a New Table

**Step 1**: Create new entity

```csharp
public class WateringHistory
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime CareTime { get; set; }
    
    // Navigation property
    public PlantDbModel Plant { get; set; }
}
```

**Step 2**: Add DbSet to context

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<PlantDbModel> Plants { get; set; }
    public DbSet<WateringHistory> WateringHistories { get; set; } // NEW
}
```

**Step 3**: Configure relationship

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<WateringHistory>()
        .HasOne(w => w.Plant)
        .WithMany(p => p.WateringHistories)
        .HasForeignKey(w => w.PlantId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

**Step 4**: Create migration

```bash
Add-Migration AddWateringHistoryTable -Project PlantCare.Data
```

#### 4. Removing a Table/Column

**Example**: Remove unused UserPreferences table

**Step 1**: Remove from DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<PlantDbModel> Plants { get; set; }
    // REMOVED: public DbSet<UserPreferences> UserPreferences { get; set; }
}
```

**Step 2**: Delete the entity class (optional but recommended)

**Step 3**: Create migration

```bash
Add-Migration RemoveUserPreferencesTable -Project PlantCare.Data
```

#### 5. Renaming Column

**Warning**: Renaming in EF creates a DROP + CREATE, which LOSES DATA!

**Wrong Way** (loses data):

```csharp
// Just renaming property
public string PlantName { get; set; } // was "Name"
```

**Right Way** (preserves data):

**Option A**: Use Column attribute

```csharp
[Column("Name")] // Keep database column name
public string PlantName { get; set; } // Rename C# property
```

**Option B**: Manually edit migration

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Instead of DropColumn + AddColumn, use RenameColumn
    migrationBuilder.RenameColumn(
        name: "Name",
        table: "Plants",
        newName: "PlantName");
}
```

---

### 🔧 Advanced Migration Scenarios

#### Data Seeding in Migration

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create table
    migrationBuilder.CreateTable(...);
    
    // Seed initial data
    migrationBuilder.InsertData(
        table: "Plants",
        columns: new[] { "Id", "Name", "Species" },
        values: new object[] { Guid.NewGuid(), "Sample Plant", "Unknown" }
    );
}
```

#### Conditional Migration (Check if column exists)

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT * FROM pragma_table_info('Plants') 
            WHERE name='Notes'
        )
        BEGIN
            ALTER TABLE Plants ADD COLUMN Notes TEXT
        END
    ");
}
```

#### Complex Data Migration

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add new column
    migrationBuilder.AddColumn<int>(
        name: "WateringFrequencyInHours",
        table: "Plants",
        nullable: false,
        defaultValue: 72);
    
    // 2. Migrate data from old column
    migrationBuilder.Sql(
        "UPDATE Plants SET WateringFrequencyInHours = WateringFrequencyInDays * 24"
    );
    
    // 3. Drop old column
    migrationBuilder.DropColumn(
        name: "WateringFrequencyInDays",
        table: "Plants");
}
```

---

### 🛠️ Developer Notes & Best Practices

#### ✅ DO's

1. **Always create migration before changing database**

   ```bash
   # RIGHT ORDER:
   # 1. Change model
   # 2. Create migration
   # 3. Run app (migration auto-applies)
   ```

2. **Use descriptive migration names**

   ```bash
   # GOOD
   Add-Migration AddPlantNotesField
   Add-Migration RemoveUserTable
   Add-Migration AddWateringHistoryTable
   
   # BAD
   Add-Migration Update1
   Add-Migration Fix
   Add-Migration temp
   ```

3. **Review generated migration before committing**
   - Check `Up()` method creates correct schema
   - Verify `Down()` method can revert changes
   - Test data migration SQL if included

4. **Keep migrations in source control**
   - Commit migration files to Git
   - Include snapshot file
   - Never delete old migrations (unless not yet released)

5. **Test migrations on clean database**

   ```bash
   # Delete database and re-run app to test all migrations
   adb shell run-as com.jianping.myplantcare.app rm files/plants.db
   ```

#### ❌ DON'Ts

1. **DON'T modify existing migrations**

   ```bash
   # If migration already applied, create a NEW migration to fix it
   # Don't edit the old one!
   ```

2. **DON'T manually edit database schema**

   ```sql
   -- Don't do this!
   ALTER TABLE Plants ADD COLUMN Notes TEXT;
   ```

   ```bash
   # Instead, let EF migrations handle it
   Add-Migration AddNotesColumn
   ```

3. **DON'T delete migration files**
   - Once applied, migrations are part of database history
   - Deleting causes "migration not found" errors

4. **DON'T use Update-Database for MAUI apps**
   - Migrations apply automatically at runtime
   - `context.Database.Migrate()` in startup code

#### 📝 Common Patterns in PlantCare

**Pattern 1: Adding care history tracking**

```bash
Add-Migration AddWateringAndFertilizationHistoryTables
```

- Creates WateringHistory table
- Creates FertilizationHistory table
- Sets up foreign keys with cascade delete

**Pattern 2: Removing unused features**

```bash
Add-Migration RemoveUnusedTable
```

- Drops table no longer needed
- EF generates clean DROP TABLE statement

**Pattern 3: Adding initial history rows**

```bash
Add-Migration AddInitialRowsToWateringHistoryAndFertilizationHistory
```

- Migrates existing timestamp data to history tables
- Uses SQL to populate new tables from existing data

---

### 🔍 Troubleshooting

#### Problem: "A migration has already been applied to the database"

**Solution**: Delete database and re-run app

```bash
# Android
adb shell run-as com.jianping.myplantcare.app rm files/plants.db

# Windows
# Delete from: C:\Users\{user}\AppData\Local\Packages\{app-id}\LocalState\plants.db

# Then re-run app
```

#### Problem: "No migrations configuration type was found"

**Solution**: Ensure DbContext is in startup project or specify project

```bash
Add-Migration MyMigration -Project PlantCare.Data -StartupProject PlantCare.Data
```

#### Problem: Migration creates DROP + CREATE instead of ALTER

**Cause**: EF can't detect rename, treats as delete + add

**Solution**: Manually edit migration to use `RenameColumn`

```csharp
migrationBuilder.RenameColumn(
    name: "OldName",
    table: "TableName",
    newName: "NewName");
```

#### Problem: "The model backing the context has changed"

**Cause**: Model changed but no migration created

**Solution**: Create migration

```bash
Add-Migration FixModelChanges -Project PlantCare.Data
```

#### Problem: Foreign key constraint failed

**Cause**: Trying to delete parent row with child rows

**Solution**: Use cascade delete or delete children first

```csharp
// In OnModelCreating
.OnDelete(DeleteBehavior.Cascade); // Auto-delete children
```

---

### 📊 Migration Workflow in PlantCare

```
1. Modify Entity Model (C# class)
   ↓
2. Add Migration
   Add-Migration DescriptiveName -Project PlantCare.Data
   ↓
3. Review Generated Files
   - Check {timestamp}_MigrationName.cs
   - Verify Up() and Down() methods
   ↓
4. Test Migration
   - Delete database
   - Run app
   - Verify schema correct
   ↓
5. Commit to Git
   git add Migrations/
   git commit -m "Add migration: DescriptiveName"
   ↓
6. Migration Auto-Applies on App Startup
   (via context.Database.Migrate() in MauiProgram.cs)
```

---

### 🎯 PlantCare Migration History

| Migration | Date | Purpose |
|-----------|------|---------|
| 20240521170322_InitialCreate | 2024-05-21 | Initial database schema |
| 20240528164442_AddFertilize | 2024-05-28 | Add fertilization tracking |
| 20240610015602_AddWateringAndFertilizationHistoryTables | 2024-06-10 | Add history tables |
| 20240612020121_RemoveUnusedTable | 2024-06-12 | Cleanup |
| 20240614022629_AddNotesColumnInPlants | 2024-06-14 | Add notes field |
| 20240614034902_AddInitialRowsToWateringHistoryAndFertilizationHistory | 2024-06-14 | Seed history data |
| 20241029022819_remove user table | 2024-10-29 | Remove user table |

---

### 💡 Quick Tips

1. **Check current migrations**

   ```bash
   # List all migrations
   Get-Migration -Project PlantCare.Data
   ```

2. **Remove last migration (if not applied yet)**

   ```bash
   Remove-Migration -Project PlantCare.Data
   ```

3. **Generate SQL script from migration**

   ```bash
   Script-Migration -From 0 -To AddNotesColumn -Project PlantCare.Data
   ```

4. **See what migration will do**

   ```bash
   # Look at Up() method in generated migration file
   # Or generate SQL:
   Script-Migration -Project PlantCare.Data
   ```

5. **Rollback to specific migration** (rarely needed in MAUI)

   ```bash
   # This is mainly for server apps with Update-Database
   # For MAUI, easier to delete database and restart
   ```

---

## Delete Database File

To find and delete the file /data/user/0/com.companyname.plantcare.app/files/PlantCareApp.db on an Android simulator, you can use the Android Debug Bridge (ADB), which is a versatile command-line tool that lets you communicate with an emulator instance.

The permission denied error indicates that the directory `/data/user/0/com.companyname.plantcare.app/files` is protected, and you don't have sufficient permissions to access it directly via ADB shell. However, there are workarounds for this issue:

### Workaround Using ADB with Root Access

In some cases, you might be able to gain root access to the emulator, which would allow you to access and delete the file. Here's how you can try this:

1. **Restart ADB as Root**:

   ```sh
   adb root
   ```

   This command restarts ADB with root permissions.

2. **Access the Shell Again**:

   ```sh
   adb shell
   ```

3. **Delete the File with Root Permissions**:

   ```sh
   rm /data/user/0/com.companyname.plantcare.app/files/PlantCareApp.db
   ```

If the `adb root` command doesn't work because the emulator is not configured to allow root access, you can use another method.

### Workaround Using the App's Context

You can modify your app's code to delete the file programmatically, which runs with the app's permissions and should have access to its own private storage.

**Step 1: Add Code to Delete the File**
Add a method to your application to delete the file:

```csharp
using System.IO;
using Microsoft.Maui.Storage; // Updated for .NET MAUI

public static class FileHelper
{
    public static void DeleteDatabaseFile()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "plants.db");
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }
}
```

**Step 2: Call This Method**
You can call this method from somewhere in your app, such as a button click handler or on app startup for testing purposes:

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        // Call the method to delete the database file
        // FileHelper.DeleteDatabaseFile(); // Uncomment to use
    }
}
```

### Quick Delete Commands

```bash
# Android - Delete database via ADB
adb shell run-as com.jianping.myplantcare.app rm files/plants.db

# Android - Delete database with root
adb root
adb shell rm /data/user/0/com.jianping.myplantcare.app/files/plants.db

# Windows - Delete from Package folder
# Delete: C:\Users\{username}\AppData\Local\Packages\{package-id}\LocalState\plants.db
```

### Summary

While direct access to the private app directory on an Android device or emulator is restricted, you can either attempt to gain root access with `adb root` or modify your app to delete the file programmatically. The latter is generally more straightforward and safer, especially for testing and debugging purposes.

If you need further assistance or additional modifications to your app, feel free to ask!

## Resources

### Material-Design-Icons

<https://github.com/google/material-design-icons>
<https://fonts.google.com/icons>

`&#x....;`

## Media

### Photos and Videos

<https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device-media/picker?view=net-maui-8.0&tabs=android>
