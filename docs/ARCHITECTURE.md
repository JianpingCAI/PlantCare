# PlantCare Architecture Documentation

## Overview

PlantCare is a .NET MAUI mobile application built with a clean architecture approach, following the MVVM (Model-View-ViewModel) pattern. The application is designed to run on Android, iOS, and Windows platforms, with primary testing on Android devices.

## Project Structure

The solution consists of three main projects:

```
PlantCare/
├── PlantCare.App/              # Main MAUI application
├── PlantCare.Data/             # Data access layer
└── PlantCare.App.Tests/        # Unit tests
```

### PlantCare.App

The main application project containing UI, ViewModels, Services, and platform-specific code.

#### Key Directories:

- **Views/** - XAML UI pages
  - `PlantOverviewView.xaml` - Main plant list view
  - `PlantDetailView.xaml` - Individual plant details
  - `PlantAddEditView.xaml` - Add/Edit plant form
  - `PlantCalendarView.xaml` - Calendar view for care schedules
  - `CareHistoryView.xaml` - Plant care history
  - `SettingsView.xaml` - Application settings

- **ViewModels/** - ViewModels implementing MVVM pattern
  - Base ViewModels: `ViewModelBase`, `PlantViewModelBase`
  - Feature ViewModels: `PlantListOverviewViewModel`, `PlantDetailViewModel`, etc.
  - Uses `CommunityToolkit.Mvvm` for MVVM implementation

- **Services/** - Application services
  - **DBService/** - Database interaction services
    - `IPlantService` / `PlantService` - Plant CRUD operations
  - **DataExportImport/** - Import/export functionality
  - **Security/** - Encryption services
  - **Accessibility/** - Accessibility features
  - `INavigationService` / `NavigationService` - Navigation management
  - `IDialogService` / `DialogService` - User dialogs
  - `INotificationService` - Local notifications
  - `IImageOptimizationService` - Image processing and optimization
  - `IAppSettingsService` - Application settings management

- **Models/** - View models and DTOs

- **Utils/** - Utility classes and helpers
  - `LocalizationManager` - Multi-language support
  - `PlantState` - Plant state calculations

- **Messaging/** - Application-wide messaging system
  - Uses `WeakReferenceMessenger` from CommunityToolkit.Mvvm
  - Messages: `PlantAddedMessage`, `PlantModifiedMessage`, `PlantDeletedMessage`, etc.

- **Resources/** - Application resources
  - **Styles/** - XAML styling resources
  - **Images/** - Image assets
  - **Fonts/** - Custom fonts
  - **LocalizationResources.resx** - Localization strings

- **Platforms/** - Platform-specific implementations
  - Android/
  - iOS/
  - Windows/

### PlantCare.Data

Data access layer implementing repository pattern with Entity Framework Core.

#### Key Components:

- **DbModels/** - Database entity models
  - `PlantDbModel` - Plant entity
  - `WateringHistory` - Watering event records
  - `FertilizationHistory` - Fertilization event records
  - `CareHistoryBase` / `EventHistoryBase` - Base classes

- **Models/** - Business models
  - `Plant` - Domain model for plants
  - `CareType` - Enum for care types

- **Repositories/** - Data access repositories
  - **interfaces/** - Repository contracts
    - `IRepository<T>` - Generic repository interface
    - `IPlantRepository` - Plant-specific operations
    - `IWateringHistoryRepository` - Watering history operations
    - `IFertilizationHistoryRepository` - Fertilization history operations
  - `GenericRepository<T>` - Base repository implementation
  - `PlantRepository` - Plant data access
  - `WateringHistoryRepository` - Watering history data access
  - `FertilizationHistoryRepository` - Fertilization history data access
  - `ApplicationDbContext` - EF Core DbContext
  - `ApplicationDbContextFactory` - Context factory for migrations

- **Migrations/** - EF Core database migrations

### PlantCare.App.Tests

Unit test project using xUnit.

- **Services/DBService/** - Service layer tests
- **Common/** - Test utilities and fixtures
  - `ServiceProviderFixture` - DI container setup for tests
  - `ServiceProviderFactory` - Test service configuration
  - `MappingProfile` - AutoMapper configuration for tests

## Architecture Patterns

### MVVM Pattern

The application strictly follows the MVVM pattern:

- **Model**: Data models and business logic (in PlantCare.Data)
- **View**: XAML pages (in Views/)
- **ViewModel**: View logic and data binding (in ViewModels/)

#### ViewModel Base Classes

```csharp
ViewModelBase
├── PlantViewModelBase
│   ├── PlantDetailViewModel
│   └── PlantListItemViewModel
├── PlantListOverviewViewModel
├── PlantAddEditViewModel
├── PlantCalendarViewModel
└── ...
```

All ViewModels inherit from `ViewModelBase` which provides:
- `IsBusy` and `IsLoading` properties
- `LoadDataWhenViewAppearingAsync()` for view lifecycle
- Property change notification via `ObservableObject`

### Repository Pattern

Data access is abstracted through repositories:

```
IRepository<T>
├── IPlantRepository
├── IWateringHistoryRepository
└── IFertilizationHistoryRepository
```

Each repository provides:
- CRUD operations
- Entity-specific queries
- Transaction management

### Dependency Injection

The application uses .NET's built-in DI container configured in `MauiProgram.cs`:

```csharp
ConfigureDatabase()          // DbContext registration
ConfigureRepositories()      // Repository registrations
ConfigureAppServices()       // Application services
ConfigureSecurityServices()  // Security services
ConfigureAccessibilityServices()  // Accessibility services
ConfigureViewsAndViewModels()     // View/ViewModel registrations
ConfigureNavigation()        // Navigation services
ConfigureDataServices()      // Data import/export services
ConfigureLogging()          // Logging configuration
```

#### Service Lifetimes:

- **Singleton**: Long-lived services and ViewModels for main views
  - `PlantService`, `AppSettingsService`, `NavigationService`
  - `PlantListOverviewViewModel`, `SettingsViewModel`

- **Transient**: New instances for each request
  - `PlantAddEditViewModel`, `PlantCalendarViewModel`
  - Data import/export services

- **Scoped**: Per-request lifetime (mainly for repositories)
  - `IPlantRepository`, `IWateringHistoryRepository`

### Messaging Pattern

The application uses `WeakReferenceMessenger` for loosely-coupled communication:

```csharp
// Send message
WeakReferenceMessenger.Default.Send(new PlantAddedMessage(plantId));

// Receive message
public class MyViewModel : IRecipient<PlantAddedMessage>
{
    void IRecipient<PlantAddedMessage>.Receive(PlantAddedMessage message)
    {
        // Handle message
    }
}
```

**Key Messages:**
- `PlantAddedMessage` - New plant created
- `PlantModifiedMessage` - Plant updated
- `PlantDeletedMessage` - Plant removed
- `PlantStateChangedMessage` - Plant care state updated
- `IsNotificationEnabledMessage` - Notification settings changed
- `DataImportMessage` - Data imported

## Data Flow

### Plant Creation Flow

```
PlantAddEditView → PlantAddEditViewModel → PlantService → PlantRepository → SQLite Database
                                                ↓
                                    WeakReferenceMessenger
                                                ↓
                                    PlantListOverviewViewModel
                                                ↓
                                    Update UI + Schedule Notifications
```

### Notification Scheduling Flow

```
PlantListOverviewViewModel → NotificationService → Plugin.LocalNotification
                                                           ↓
                                                   Platform-specific
                                                   notification APIs
```

### Navigation Flow

```
View → ViewModel → NavigationService → Shell Navigation → Target View
```

## Data Storage

### Local Database

- **Technology**: SQLite via Entity Framework Core
- **Location**: `FileSystem.AppDataDirectory/plants.db`
- **Migrations**: Code-first migrations in PlantCare.Data/Migrations

### Database Schema

```sql
Plants
├── Id (Guid, PK)
├── Name (string)
├── Species (string)
├── Age (int)
├── PhotoPath (string)
├── LastWatered (DateTime)
├── WateringFrequencyInHours (int)
├── LastFertilized (DateTime)
├── FertilizeFrequencyInHours (int)
└── Notes (string)

WateringHistory
├── Id (Guid, PK)
├── PlantId (Guid, FK → Plants)
└── CareTime (DateTime)

FertilizationHistory
├── Id (Guid, PK)
├── PlantId (Guid, FK → Plants)
└── CareTime (DateTime)
```

**Cascade Delete**: Deleting a plant automatically deletes its history records.

### Image Storage

- **Full Images**: Stored in `FileSystem.AppDataDirectory` with optimized dimensions
- **Thumbnails**: Generated automatically for list views
- **Optimization**: Handled by `IImageOptimizationService`
- **Default Image**: Embedded resource for plants without photos

### Application Settings

- **Technology**: .NET MAUI Preferences API
- **Settings Managed by `IAppSettingsService`**:
  - Watering notification enabled/disabled
  - Fertilization notification enabled/disabled
  - Language preference
  - Theme settings

## Key Features Implementation

### 1. Notifications

**Technology**: `Plugin.LocalNotification`

**Flow**:
1. When plant added/modified → Schedule notifications
2. Calculate next watering/fertilization time
3. Create `NotificationRequest` with unique ID
4. Schedule via `INotificationService`
5. Handle notification tap → Navigate to plant detail

**Notification ID Strategy**:
- Watering: `plantId.GetHashCode()`
- Fertilization: `-plantId.GetHashCode()`

### 2. Image Optimization

**Service**: `IImageOptimizationService`

**Process**:
1. User selects/captures image
2. Load original image stream
3. Resize to maximum dimensions (preserving aspect ratio)
4. Generate thumbnail version
5. Save both to app data directory
6. Update plant record with path

### 3. Data Import/Export

**Services**: `IDataExportService`, `IDataImportService`

**Export**:
1. Serialize plants and history to JSON
2. Copy photos to export directory
3. Create zip archive
4. Save to user-selected location

**Import**:
1. Extract zip archive
2. Deserialize JSON data
3. Copy photos to app directory
4. Insert into database
5. Broadcast `DataImportMessage`

### 4. Localization

**Technology**: .resx resource files + `LocalizationManager`

**Supported Languages**:
- English (default)
- Chinese (zh-CN)

**Usage**:
```csharp
LocalizationManager.Instance["KeyName"]
```

### 5. Calendar View

**Technology**: XCalendar.Maui

**Features**:
- Visual calendar with care events
- Different colors for watering/fertilization
- Event details on date selection

### 6. Care History Tracking

**Implementation**:
- Separate tables for watering and fertilization history
- Automatically updated when care action performed
- Chart visualization using LiveChartsCore.SkiaSharpView.Maui

## Technology Stack

### Frameworks & Libraries

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| Framework | .NET MAUI | 10.0 | Cross-platform UI framework |
| Database | Entity Framework Core | 10.0 | ORM for SQLite |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 | MVVM helpers and messaging |
| UI Toolkit | CommunityToolkit.Maui | 13.0.0 | Additional MAUI controls |
| Mapping | AutoMapper | 12.0.1 | Object-to-object mapping |
| Notifications | Plugin.LocalNotification | 12.0.0 | Local push notifications |
| Logging | Serilog | 4.3.0 | File-based logging |
| Charts | LiveChartsCore.SkiaSharpView.Maui | 2.0.0-rc6.1 | Data visualization |
| Calendar | XCalendar.Maui | 4.6.0 | Calendar UI component |
| Graphics | SkiaSharp | 3.119.1 | Image processing |
| Security | System.Security.Cryptography | 10.0.1 | Data encryption |
| Testing | xUnit | Latest | Unit testing framework |

### Platform Targets

- **Android**: API 21+ (Android 5.0+)
- **iOS**: iOS 11.0+
- **Windows**: Windows 10.0.19041.0+
- **macCatalyst**: macOS 13.1+ (optional)

## Logging Strategy

**Logger**: Serilog with file sink

**Configuration**:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Month)
    .CreateLogger();
```

**Log Location**: `FileSystem.AppDataDirectory/app-{date}.log`

**Custom Logger**: `IAppLogger<T>` wrapper for dependency injection

**Usage**:
```csharp
_logger.LogInformation("Plant {Name} added", plant.Name);
_logger.LogError(ex, "Error updating plant");
```

## Navigation Architecture

**Shell-based Navigation**:

The app uses `Shell` for navigation with the following structure:

```
AppShell
├── Tab: Home (PlantOverviewView)
├── Tab: Calendar (PlantCalendarView)
├── Tab: History (CareHistoryView)
└── Tab: Settings (SettingsView)

Routes:
├── //Overview → PlantOverviewView
├── //Plant → PlantDetailView
├── //Edit → PlantAddEditView
├── //Add → PlantAddEditView
├── //Calendar → PlantCalendarView
├── //SinglePlantCareHistory → SingePlantCareHistoryView
├── //About → AboutPage
└── //LogViewer → LogViewerPage
```

**Navigation Service**:
- Centralized navigation logic in `INavigationService`
- Parameter passing via query attributes
- Backward navigation support

## Security Considerations

### Data Encryption

**Service**: `IEncryptionService`

- Sensitive data can be encrypted using platform-specific APIs
- Uses `System.Security.Cryptography.ProtectedData` where available

### Permissions

**Android**:
- Camera access (for taking photos)
- External storage (for photo picking)
- Notifications (for care reminders)

**iOS**:
- Photo library access
- Camera access
- Notification permission

## Performance Optimizations

1. **Image Optimization**:
   - Resize images to reasonable dimensions
   - Generate thumbnails for list views
   - Lazy loading of images

2. **Database**:
   - Efficient queries with EF Core
   - Cascade deletes configured
   - Async operations throughout

3. **UI**:
   - Virtualized collection views
   - Minimal re-renders with MVVM
   - Async data loading with loading indicators

4. **Caching**:
   - Plant list cached in ViewModel
   - Image paths cached (not reloaded)

## Testing Strategy

### Unit Tests

Located in `PlantCare.App.Tests`:

- **Service Tests**: Business logic validation
- **Repository Tests**: Database operations
- **ViewModel Tests**: UI logic (to be expanded)

**Test Infrastructure**:
- xUnit test framework
- Dependency injection for tests
- In-memory SQLite for repository tests

### Manual Testing

Currently tested on:
- Android devices (primary platform)
- iOS (limited testing)
- Windows (limited testing)

## Future Architecture Considerations

### Potential Improvements

1. **Cloud Sync**: Add optional cloud backup via Azure Mobile Apps or Firebase
2. **Offline-first**: Already implemented, consider sync conflict resolution
3. **Plugin Architecture**: Allow third-party plant care modules
4. **CQRS Pattern**: Separate read/write models for complex operations
5. **State Management**: Consider centralized state management for larger scale

### Scalability

The current architecture supports:
- Hundreds of plants (tested)
- Multiple users (not yet implemented)
- Plugin extensibility (planned)

## Conclusion

PlantCare follows modern .NET MAUI best practices with clean separation of concerns, testability, and maintainability as core principles. The architecture supports cross-platform development while allowing platform-specific optimizations where needed.
