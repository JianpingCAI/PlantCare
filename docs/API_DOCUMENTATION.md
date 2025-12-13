# PlantCare API Documentation

## Overview

This document describes the internal APIs and services used within the PlantCare application. PlantCare is a local-first application with no remote API endpoints. All operations are performed locally on the device.

## Service Layer APIs

### IPlantService

Plant management service providing CRUD operations and care tracking.

**Location**: `PlantCare.App/Services/DBService/IPlantService.cs`

#### Methods

##### CreatePlantAsync
Creates a new plant in the database.

```csharp
Task<Guid> CreatePlantAsync(PlantDbModel plant)
```

**Parameters**:
- `plant` (PlantDbModel): Plant data to create

**Returns**: `Task<Guid>` - ID of the created plant

**Throws**:
- `ArgumentNullException` - If plant is null
- `DbUpdateException` - If database operation fails

**Example**:
```csharp
var plant = new PlantDbModel
{
    Name = "Rose",
    Species = "Rosa",
    Age = 2,
    LastWatered = DateTime.Now,
    WateringFrequencyInHours = 48
};
var plantId = await _plantService.CreatePlantAsync(plant);
```

---

##### GetAllPlantsAsync
Retrieves all plants ordered by name.

```csharp
Task<List<Plant>> GetAllPlantsAsync()
```

**Returns**: `Task<List<Plant>>` - List of all plants

**Example**:
```csharp
var plants = await _plantService.GetAllPlantsAsync();
foreach (var plant in plants)
{
    Console.WriteLine(plant.Name);
}
```

---

##### GetPlantByIdAsync
Gets a specific plant by ID.

```csharp
Task<Plant?> GetPlantByIdAsync(Guid id)
```

**Parameters**:
- `id` (Guid): Plant identifier

**Returns**: `Task<Plant?>` - Plant if found, null otherwise

**Example**:
```csharp
var plant = await _plantService.GetPlantByIdAsync(plantId);
if (plant != null)
{
    Console.WriteLine($"Found: {plant.Name}");
}
```

---

##### UpdatePlantAsync
Updates an existing plant.

```csharp
Task<bool> UpdatePlantAsync(PlantDbModel plant)
```

**Parameters**:
- `plant` (PlantDbModel): Updated plant data

**Returns**: `Task<bool>` - True if update successful, false otherwise

**Example**:
```csharp
plant.Name = "Updated Name";
bool success = await _plantService.UpdatePlantAsync(plant);
```

---

##### DeletePlantAsync
Deletes a plant and all its history records.

```csharp
Task DeletePlantAsync(Guid plantId)
```

**Parameters**:
- `plantId` (Guid): ID of plant to delete

**Note**: This operation cascades to delete all watering and fertilization history.

**Example**:
```csharp
await _plantService.DeletePlantAsync(plantId);
```

---

##### UpdateLastWateringTime
Updates the last watering timestamp for a plant.

```csharp
Task UpdateLastWateringTime(Guid plantId, DateTime time)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `time` (DateTime): New watering timestamp

---

##### UpdateLastFertilizationTime
Updates the last fertilization timestamp for a plant.

```csharp
Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `time` (DateTime): New fertilization timestamp

---

##### AddWateringHistoryAsync
Adds a watering event to plant history.

```csharp
Task AddWateringHistoryAsync(Guid plantId, DateTime lastWatered)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `lastWatered` (DateTime): Watering timestamp

**Example**:
```csharp
await _plantService.AddWateringHistoryAsync(plantId, DateTime.Now);
```

---

##### DeleteWateringHistoryAsync
Removes a watering history record.

```csharp
Task DeleteWateringHistoryAsync(Guid plantId, Guid historyId)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `historyId` (Guid): History record ID

**Behavior**: If the deleted record was the most recent, updates plant's LastWatered to the next most recent record.

---

##### AddFertilizationHistoryAsync
Adds a fertilization event to plant history.

```csharp
Task AddFertilizationHistoryAsync(Guid plantId, DateTime lastFertilized)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `lastFertilized` (DateTime): Fertilization timestamp

---

##### DeleteFertilizationHistoryAsync
Removes a fertilization history record.

```csharp
Task DeleteFertilizationHistoryAsync(Guid plantId, Guid historyId)
```

**Parameters**:
- `plantId` (Guid): Plant identifier
- `historyId` (Guid): History record ID

---

##### GetAllPlantsWithCareHistoryAsync
Gets all plants with complete care history.

```csharp
Task<List<PlantCareHistory>> GetAllPlantsWithCareHistoryAsync()
```

**Returns**: `Task<List<PlantCareHistory>>` - Plants with watering and fertilization timestamps

**Example**:
```csharp
var plantsWithHistory = await _plantService.GetAllPlantsWithCareHistoryAsync();
foreach (var plant in plantsWithHistory)
{
    Console.WriteLine($"{plant.Name}: {plant.WateringTimestamps.Count} waterings");
}
```

---

##### AddPlantsAsync
Bulk insert multiple plants.

```csharp
Task AddPlantsAsync(List<PlantDbModel> plants)
```

**Parameters**:
- `plants` (List<PlantDbModel>): List of plants to add

**Use Case**: Data import functionality

---

##### ClearAllTablesAsync
Deletes all data from all tables.

```csharp
Task ClearAllTablesAsync()
```

**Warning**: This is a destructive operation. Use with caution.

---

##### DeleteAllPhotosAsync
Deletes all plant photos from storage.

```csharp
Task DeleteAllPhotosAsync()
```

---

### INavigationService

Navigation management service.

**Location**: `PlantCare.App/Services/INavigationService.cs`

#### Methods

##### GoToPlantsOverview
Navigate to plants overview page.

```csharp
Task GoToPlantsOverview()
```

---

##### GoToPlantDetail
Navigate to plant detail page.

```csharp
Task GoToPlantDetail(Guid plantId)
```

**Parameters**:
- `plantId` (Guid): ID of plant to view

---

##### GoToAddPlant
Navigate to add plant page.

```csharp
Task GoToAddPlant(int plantCount)
```

**Parameters**:
- `plantCount` (int): Current number of plants (for default naming)

---

##### GoToEditPlant
Navigate to edit plant page.

```csharp
Task GoToEditPlant(Plant plant)
```

**Parameters**:
- `plant` (Plant): Plant to edit

---

##### GoBack
Navigate back to previous page.

```csharp
Task GoBack()
```

---

### IDialogService

User dialog service.

**Location**: `PlantCare.App/Services/IDialogService.cs`

#### Methods

##### Notify
Display an information message to the user.

```csharp
Task Notify(string title, string message, string buttonText = "OK")
```

**Parameters**:
- `title` (string): Dialog title
- `message` (string): Dialog message
- `buttonText` (string): Button text (default: "OK")

**Example**:
```csharp
await _dialogService.Notify("Success", "Plant added successfully");
```

---

##### Ask
Display a confirmation dialog.

```csharp
Task<bool> Ask(string title, string message, string acceptText = "Yes", string cancelText = "No")
```

**Parameters**:
- `title` (string): Dialog title
- `message` (string): Confirmation message
- `acceptText` (string): Accept button text (default: "Yes")
- `cancelText` (string): Cancel button text (default: "No")

**Returns**: `Task<bool>` - True if user accepted, false otherwise

**Example**:
```csharp
bool confirmed = await _dialogService.Ask(
    "Confirm Delete",
    "Delete this plant?",
    "Delete",
    "Cancel"
);
if (confirmed)
{
    await _plantService.DeletePlantAsync(plantId);
}
```

---

### INotificationService

Local notification service.

**Location**: Wrapper around `Plugin.LocalNotification`

#### Properties

##### IsSupported
Indicates if notifications are supported on the current platform.

```csharp
bool IsSupported { get; }
```

#### Methods

##### Show
Schedule a local notification.

```csharp
Task Show(NotificationRequest request)
```

**Parameters**:
- `request` (NotificationRequest): Notification configuration

**Example**:
```csharp
var notification = new NotificationRequest
{
    NotificationId = 1,
    Title = "Water Your Plant",
    Description = "Time to water Rose",
    Schedule = new NotificationRequestSchedule
    {
        NotifyTime = DateTime.Now.AddHours(24)
    }
};
await _notificationService.Show(notification);
```

---

##### Cancel
Cancel scheduled notifications.

```csharp
void Cancel(params int[] notificationIds)
```

**Parameters**:
- `notificationIds` (int[]): IDs of notifications to cancel

---

##### CancelAll
Cancel all scheduled notifications.

```csharp
void CancelAll()
```

---

##### GetPendingNotificationList
Get list of pending (scheduled but not shown) notifications.

```csharp
Task<IList<NotificationRequest>> GetPendingNotificationList()
```

**Returns**: List of pending notifications

---

##### AreNotificationsEnabled
Check if notification permission is granted.

```csharp
Task<bool> AreNotificationsEnabled()
```

**Returns**: True if enabled, false otherwise

---

##### RequestNotificationPermission
Request notification permission from user.

```csharp
Task<bool> RequestNotificationPermission()
```

**Returns**: True if granted, false otherwise

---

### IAppSettingsService

Application settings management.

**Location**: `PlantCare.App/Services/IAppSettingsService.cs`

#### Methods

##### GetWateringNotificationSettingAsync
Get watering notification preference.

```csharp
Task<bool> GetWateringNotificationSettingAsync()
```

**Returns**: True if watering notifications enabled

---

##### SetWateringNotificationSettingAsync
Set watering notification preference.

```csharp
Task SetWateringNotificationSettingAsync(bool enabled)
```

---

##### GetFertilizationNotificationSettingAsync
Get fertilization notification preference.

```csharp
Task<bool> GetFertilizationNotificationSettingAsync()
```

---

##### SetFertilizationNotificationSettingAsync
Set fertilization notification preference.

```csharp
Task SetFertilizationNotificationSettingAsync(bool enabled)
```

---

### IImageOptimizationService

Image processing and optimization service.

**Location**: `PlantCare.App/Services/IImageOptimizationService.cs`

#### Methods

##### OptimizeAndSaveImageAsync
Optimize and save an image with thumbnail generation.

```csharp
Task<string> OptimizeAndSaveImageAsync(Stream sourceStream, string fileName)
```

**Parameters**:
- `sourceStream` (Stream): Source image stream
- `fileName` (string): Desired file name

**Returns**: Path to saved optimized image

**Behavior**:
- Resizes image to maximum dimensions (preserving aspect ratio)
- Generates thumbnail version
- Saves both to app data directory
- Returns path to full-size image

**Example**:
```csharp
using var stream = await photo.OpenReadAsync();
string imagePath = await _imageOptimizationService
    .OptimizeAndSaveImageAsync(stream, "plant_photo.jpg");
```

---

##### DeleteImageAndThumbnailAsync
Delete both image and its thumbnail.

```csharp
Task DeleteImageAndThumbnailAsync(string imagePath)
```

**Parameters**:
- `imagePath` (string): Path to image

---

##### GetThumbnailPath
Get path to thumbnail for an image.

```csharp
string GetThumbnailPath(string imagePath)
```

**Parameters**:
- `imagePath` (string): Path to full-size image

**Returns**: Path to thumbnail image

---

### IDataExportService

Data export functionality.

**Location**: `PlantCare.App/Services/DataExportImport/IDataExportService.cs`

#### Methods

##### ExportDataAsync
Export all plant data and photos to ZIP file.

```csharp
Task<string> ExportDataAsync(string destinationPath)
```

**Parameters**:
- `destinationPath` (string): Where to save the export file

**Returns**: Path to created ZIP file

**Export Contents**:
- `plants.json` - Plant data
- `watering_history.json` - Watering records
- `fertilization_history.json` - Fertilization records
- `photos/` - All plant photos

**Example**:
```csharp
string exportPath = await _exportService.ExportDataAsync("/storage/exports/");
await _dialogService.Notify("Success", $"Data exported to {exportPath}");
```

---

### IDataImportService

Data import functionality.

**Location**: `PlantCare.App/Services/DataExportImport/IDataImportService.cs`

#### Methods

##### ImportDataAsync
Import plant data from ZIP file.

```csharp
Task ImportDataAsync(string zipFilePath, bool clearExisting = true)
```

**Parameters**:
- `zipFilePath` (string): Path to import ZIP file
- `clearExisting` (bool): Whether to clear existing data first (default: true)

**Throws**:
- `FileNotFoundException` - If ZIP file not found
- `InvalidDataException` - If ZIP contents are invalid

**Example**:
```csharp
try
{
    await _importService.ImportDataAsync("/storage/backup.zip");
    await _dialogService.Notify("Success", "Data imported successfully");
}
catch (Exception ex)
{
    await _dialogService.Notify("Error", $"Import failed: {ex.Message}");
}
```

---

## Repository Layer APIs

### IPlantRepository

Low-level database access for plants.

**Location**: `PlantCare.Data/Repositories/interfaces/IPlantRepository.cs`

Extends `IRepository<PlantDbModel>` with additional methods:

#### Methods

##### GetAllPlantsWithCareHistoryAsync
Get all plants with navigation properties loaded.

```csharp
Task<List<PlantDbModel>> GetAllPlantsWithCareHistoryAsync()
```

---

##### UpdateLastWateringTime
Direct update of last watering time.

```csharp
Task UpdateLastWateringTime(Guid plantId, DateTime time)
```

---

##### UpdateLastFertilizationTime
Direct update of last fertilization time.

```csharp
Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
```

---

##### GetAllPhotoPathsAsync
Get all photo paths from database.

```csharp
Task<List<string>> GetAllPhotoPathsAsync()
```

---

##### AddPlantsAsync
Bulk insert plants.

```csharp
Task AddPlantsAsync(List<PlantDbModel> plants)
```

---

##### ClearAllTablesAsync
Delete all data.

```csharp
Task ClearAllTablesAsync()
```

---

### IWateringHistoryRepository

Watering history data access.

**Location**: `PlantCare.Data/Repositories/interfaces/IWateringHistoryRepository.cs`

#### Methods

##### GetWateringHistoryByPlantIdAsync
Get all watering records for a plant.

```csharp
Task<List<WateringHistory>> GetWateringHistoryByPlantIdAsync(Guid plantId)
```

---

### IFertilizationHistoryRepository

Fertilization history data access.

**Location**: `PlantCare.Data/Repositories/interfaces/IFertilizationHistoryRepository.cs`

#### Methods

##### GetFertilizationHistoryByPlantIdAsync
Get all fertilization records for a plant.

```csharp
Task<List<FertilizationHistory>> GetFertilizationHistoryByPlantIdAsync(Guid plantId)
```

---

## Messaging APIs

### Messenger System

PlantCare uses `WeakReferenceMessenger` from CommunityToolkit.Mvvm for loosely-coupled communication.

**Location**: `PlantCare.App/Messaging/`

#### Available Messages

##### PlantAddedMessage
Sent when a new plant is created.

```csharp
public record PlantAddedMessage(Guid PlantId);
```

**Usage**:
```csharp
// Send
WeakReferenceMessenger.Default.Send(new PlantAddedMessage(plantId));

// Receive
public class MyViewModel : IRecipient<PlantAddedMessage>
{
    public MyViewModel()
    {
        WeakReferenceMessenger.Default.Register<PlantAddedMessage>(this);
    }

    void IRecipient<PlantAddedMessage>.Receive(PlantAddedMessage message)
    {
        // Handle new plant
    }
}
```

---

##### PlantModifiedMessage
Sent when a plant is updated.

```csharp
public record PlantModifiedMessage(Guid PlantId);
```

---

##### PlantDeletedMessage
Sent when a plant is deleted.

```csharp
public record PlantDeletedMessage(Guid PlantId, string Name);
```

---

##### PlantStateChangedMessage
Sent when plant care state changes (watered/fertilized).

```csharp
public record PlantStateChangedMessage(Guid PlantId, CareType ReminderType, DateTime UpdatedTime);
```

---

##### IsNotificationEnabledMessage
Sent when notification settings change.

```csharp
public record IsNotificationEnabledMessage(CareType ReminderType, bool IsNotificationEnabled);
```

---

##### PlantCareHistoryChangedMessage
Sent when care history is modified.

```csharp
public record PlantCareHistoryChangedMessage(Guid PlantId, CareType CareType);
```

---

##### DataImportMessage
Sent when data is imported.

```csharp
public record DataImportMessage();
```

---

## Utility APIs

### LocalizationManager

Manages application localization.

**Location**: `PlantCare.App/Utils/LocalizationManager.cs`

#### Properties

##### Instance
Singleton instance.

```csharp
static LocalizationManager Instance { get; }
```

##### CurrentCulture
Get or set current culture.

```csharp
CultureInfo CurrentCulture { get; set; }
```

#### Indexer

Get localized string by key.

```csharp
string? this[string key] { get; }
```

**Example**:
```csharp
string errorMessage = LocalizationManager.Instance["Error"] ?? "Error";
```

#### Events

##### LanguageChanged
Fired when language changes.

```csharp
event EventHandler LanguageChanged;
```

---

## Error Handling

All service methods may throw the following exceptions:

- `ArgumentNullException` - Required parameter is null
- `ArgumentException` - Invalid parameter value
- `InvalidOperationException` - Operation not valid in current state
- `DbUpdateException` - Database operation failed
- `IOException` - File operation failed
- `UnauthorizedAccessException` - Permission denied

**Best Practice**:
```csharp
try
{
    await _plantService.CreatePlantAsync(plant);
}
catch (ArgumentNullException ex)
{
    await _dialogService.Notify("Error", "Plant data is required");
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Failed to create plant");
    await _dialogService.Notify("Error", "Database error occurred");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    await _dialogService.Notify("Error", "An unexpected error occurred");
}
```

---

## Versioning

**Current Version**: 0.7.0

The internal API follows semantic versioning:
- **Major**: Breaking changes to interfaces
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes

---

## Notes

- All async methods should be awaited
- Dispose of DbContext properly (handled by DI)
- Use dependency injection for service access
- Follow repository pattern for data access
- Use messaging for cross-ViewModel communication

---

**Last Updated**: 2024-XX-XX  
**API Version**: 0.7.0
