# PlantCare Architecture Documentation

## Overview

PlantCare is a .NET MAUI mobile application built with a clean architecture approach, following the MVVM (Model-View-ViewModel) pattern. The application targets .NET 10 and is designed to run on Android, iOS, Windows (and optionally MacCatalyst). Primary manual testing is performed on Android devices.

## Project Structure

The solution consists of three main projects:

```
PlantCare/
├── PlantCare.App/              # Main MAUI application
├── PlantCare.Data/             # Data access layer
└── PlantCare.App.Tests/        # Unit tests
```

### PlantCare.App

The main application project contains UI, ViewModels, Services, and platform-specific code.

#### Key Directories

- `Views/` - XAML UI pages
  - `PlantOverviewView.xaml` - Main plant list view
  - `PlantDetailView.xaml` - Individual plant details
  - `PlantAddEditView.xaml` - Add/Edit plant form
  - `PlantCalendarView.xaml` - Calendar view for care schedules
  - `CareHistoryView.xaml` - Plant care history
  - `SettingsView.xaml` - Application settings
  - `AboutPage.xaml` - About page
  - `LogViewerPage.xaml` - Application log viewer
  - `SingePlantCareHistoryView.xaml` - Single-plant history view

- `ViewModels/` - ViewModels implementing MVVM pattern
  - Base ViewModels: `ViewModelBase`, `PlantViewModelBase`
  - Feature ViewModels: `PlantListOverviewViewModel`, `PlantDetailViewModel`, etc.
  - Uses `CommunityToolkit.Mvvm` for MVVM implementation

- `Services/` - Application services
  - `DBService/` - Database interaction services
    - `IPlantService` / `PlantService` - Plant CRUD operations
  - `DataExportImport/` - Import/export functionality
  - `Security/` - Encryption services (`IEncryptionService`)
  - `Accessibility/` - Accessibility features
  - `INavigationService` / `NavigationService` - Navigation management
  - `IDialogService` / `DialogService` - User dialogs
  - `INotificationService` - Local notifications (schedules notifications using the configured notification plugin)
  - `IImageOptimizationService` - Image processing and optimization
  - `IAppSettingsService` - Application settings management

- `Components/` - Reusable XAML components (e.g. `LoadingOverlayView`, `BottomSheetView`, `InAppToastView`, `ShimmerView`, `SkeletonCardView`)

- `Resources/` - Application resources
  - `Styles/` - XAML styling resources
  - `Images/` - Image assets
  - `Fonts/` - Custom fonts
  - `LocalizationResources.resx` / `LocalizationResources.zh-CN.resx` - Localization strings
  - `Raw/` - Raw assets

- `Platforms/` - Platform-specific implementations
  - `Android/`
  - `iOS/`
  - `Windows/`

### PlantCare.Data

Data access layer implementing repository pattern with Entity Framework Core.

#### Key Components

- `DbModels/` - Database entity models
  - `PlantDbModel` - Plant entity
  - `WateringHistory` - Watering event records
  - `FertilizationHistory` - Fertilization event records
  - `CareHistoryBase` / `EventHistoryBase` - Base classes

- `Models/` - Business models
  - `Plant` - Domain model for plants
  - `CareType` - Enum for care types

- `Repositories/` - Data access repositories
  - `interfaces/` - Repository contracts
    - `IRepository<T>` - Generic repository interface
    - `IPlantRepository` - Plant-specific operations
    - `IWateringHistoryRepository` - Watering history operations
    - `IFertilizationHistoryRepository` - Fertilization history operations
  - `GenericRepository<T>` - Base repository implementation
  - `PlantRepository`, `WateringHistoryRepository`, `FertilizationHistoryRepository`
  - `ApplicationDbContext` - EF Core DbContext
  - `ApplicationDbContextFactory` - Context factory for migrations

- `Migrations/` - EF Core database migrations

### PlantCare.App.Tests

Unit test project using xUnit.

- `Services/DBService/` - Service layer tests
- `Common/` - Test utilities and fixtures
  - `ServiceProviderFixture` - DI container setup for tests
  - `ServiceProviderFactory` - Test service configuration
  - `MappingProfile` - AutoMapper configuration for tests

## Architecture Patterns

### MVVM Pattern

The application follows the MVVM pattern:

- `Model`: Data models and business logic (in `PlantCare.Data`)
- `View`: XAML pages (in `Views/`)
- `ViewModel`: View logic and data binding (in `ViewModels/`)

All ViewModels inherit from `ViewModelBase` which provides common properties and lifecycle helpers such as `IsBusy`, `IsLoading`, and `LoadDataWhenViewAppearingAsync()`; property change notification is handled by `ObservableObject` (CommunityToolkit).

### Repository Pattern

Data access is abstracted through repositories:

```
IRepository<T>
├── IPlantRepository
├── IWateringHistoryRepository
└── IFertilizationHistoryRepository
```

Each repository provides CRUD operations, entity-specific queries, and transaction management.

### Dependency Injection

The application uses .NET's built-in DI container configured in `MauiProgram.cs` and registers database contexts, repositories, application services, navigation, and platform services.

Typical registration steps in `MauiProgram.cs`:

- Configure database (`ApplicationDbContext` / factory)
- Register repositories
- Register app services (plant service, image optimization, notification service, settings, etc.)
- Register views and viewmodels
- Configure logging

#### Service Lifetimes

- `Singleton`: Long-lived services and some viewmodels (e.g. `PlantService`, `AppSettingsService`, `NavigationService`, `PlantListOverviewViewModel`, `SettingsViewModel`)
- `Transient`: Short-lived viewmodels and data import/export services (e.g. `PlantAddEditViewModel`, `PlantCalendarViewModel`)
- `Scoped`: Per-request lifetime used for repositories when appropriate

### Messaging Pattern

The application uses `WeakReferenceMessenger` (CommunityToolkit.Mvvm) for loosely-coupled communication between components.

Example messages used across the app:

- `PlantAddedMessage`
- `PlantModifiedMessage`
- `PlantDeletedMessage`
- `PlantStateChangedMessage`
- `IsNotificationEnabledMessage`
- `DataImportMessage`

## Data Flow

### Plant Creation Flow

```
PlantAddEditView → PlantAddEditViewModel → PlantService → PlantRepository → SQLite Database
                                                ↓
                                    WeakReferenceMessenger
                                                ↓
                                    PlantListOverviewViewModel → Update UI + Schedule Notifications
```

### Notification Scheduling Flow

```
PlantListOverviewViewModel → NotificationService → Notification plugin → Platform-specific notification APIs
```

### Navigation Flow

```
View → ViewModel → NavigationService → Shell Navigation → Target View
```

## Data Storage

### Local Database

- `Technology`: SQLite via Entity Framework Core (EF Core)
- `Location`: `FileSystem.AppDataDirectory/plants.db`
- `Migrations`: Code-first migrations in `PlantCare.Data/Migrations`

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

Cascade delete is configured so deleting a plant removes its history records.

### Image Storage

- Full images are stored in `FileSystem.AppDataDirectory` with optimized dimensions.
- Thumbnails are generated for list views.
- Image optimization is handled by `IImageOptimizationService` (resize, thumbnail generation, save to app data directory).
- A default embedded image is used for plants without photos.

### Application Settings

- `Technology`: .NET MAUI Preferences API
- `IAppSettingsService` manages settings such as watering/fertilization notifications, language preference, and theme.

## Key Features Implementation

### 1. Notifications

- `Notification plugin`: `Plugin.LocalNotification` (project uses v13.x)
- Flow:
  1. When a plant is added/modified → schedule notifications
  2. Calculate next watering/fertilization time
  3. Create `NotificationRequest` with unique ID and schedule it via the notification service
  4. Handle notification taps to navigate to plant detail

- Notification ID strategy commonly used in the app:
  - Watering: `plantId.GetHashCode()`
  - Fertilization: `-plantId.GetHashCode()`

### 2. Image Optimization

- `IImageOptimizationService` resizes images, preserves aspect ratio, generates thumbnails, and saves images to the app data directory. The plant record stores the image path.

### 3. Data Import/Export

- Services: `IDataExportService`, `IDataImportService`
- Export serializes plants and histories to JSON, copies photos, and creates a zip archive for user export.
- Import extracts archives, deserializes JSON, copies photos into app storage, inserts records into the database, and broadcasts a `DataImportMessage`.

### 4. Localization

- Implemented with `.resx` resource files and a `LocalizationManager`.
- Supported languages in the repo include English (default) and Chinese (`zh-CN`).
- Usage: `LocalizationManager.Instance["KeyName"]`

### 5. Calendar View

- The project currently uses `Plugin.Maui.Calendar` (in the app project) rather than `XCalendar.Maui` referenced previously in documentation. The calendar view displays care events with different colors for watering/fertilization and provides event details on date selection.

### 6. Care History Tracking

- Watering and fertilization history are stored in separate tables and updated when a care action is performed.
- Charts (where used) rely on `LiveChartsCore.SkiaSharpView.Maui` for visualization.

## Technology Stack

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| Framework | .NET MAUI | 10.0 | Cross-platform UI framework |
| Database | Microsoft.EntityFrameworkCore.Sqlite | 10.0.1 | ORM for SQLite |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 | MVVM helpers and messaging |
| UI Toolkit | CommunityToolkit.Maui | 13.0.0 | Additional MAUI controls |
| Mapping | AutoMapper | 12.0.1 | Object-to-object mapping |
| Notifications | Plugin.LocalNotification | 13.0.0 | Local push notifications |
| Logging | Serilog | 4.3.0 | File-based logging |
| Charts | LiveChartsCore.SkiaSharpView.Maui | 2.0.0-rc6.1 | Data visualization |
| Calendar | Plugin.Maui.Calendar | 2.0.1 | Calendar UI component used in this repo |
| Graphics | SkiaSharp.Views.Maui.Controls | 3.119.1 | Image/graphics rendering |
| Drawing | System.Drawing.Common | 10.0.1 | Image manipulation helpers |
| Security | System.Security.Cryptography.ProtectedData | 10.0.1 | Data encryption helpers |
| Testing | xUnit | Latest | Unit testing framework |

### Platform Targets

- `Android`: API 21+ (Android 5.0+)
- `iOS`: iOS 11.0+
- `Windows`: Windows 10.0.19041.0+ (project may include a slightly newer Windows target in multi-targeting)
- `MacCatalyst`: macOS 13.1+ (optional)

## Logging Strategy

- `Logger`: Serilog with file sink
- Configuration example used in app initializes `Log.Logger` and writes rolling logs to the app data directory. Logs are stored under `FileSystem.AppDataDirectory`.
- A small `IAppLogger<T>` wrapper is available for DI-based logging usage across the app.

## Navigation Architecture

The app uses `Shell` for navigation with tabs for Overview, Calendar, History, and Settings. Routes include (`//Overview`, `//Plant`, `//Edit`, `//Add`, `//Calendar`, `//SinglePlantCareHistory`, `//About`, `//LogViewer`).

A central `INavigationService` encapsulates Shell navigation and parameter passing.

## Security Considerations

### Data Encryption

- `IEncryptionService` provides platform-aware encryption for sensitive data. The solution references `System.Security.Cryptography.ProtectedData` for available platforms.

### Permissions

- `Android`: Camera, storage access (for photo picking), notifications
- `iOS`: Photo library, camera, notification permission

## Performance Optimizations

1. Image optimization (resizing, thumbnail generation)
2. Efficient EF Core queries and async operations
3. Virtualized collection views and minimized re-renders
4. Local caching for plant lists and image paths

## Testing Strategy

### Unit Tests

Located in `PlantCare.App.Tests` and covering services, repositories, and viewmodels where applicable. Tests use DI and in-memory or test SQLite configurations.

### Manual Testing

Primary manual testing occurs on Android devices; iOS and Windows are also verified where available.

## Future Architecture Considerations

- Cloud sync (optional backup/sync)
- Conflict resolution for offline-first sync
- Plugin architecture for extensibility
- Consider CQRS or centralized state management for larger scale

## Conclusion

PlantCare follows .NET MAUI best practices with clean separation of concerns, testability, and maintainability. The repository is actively maintained and targets .NET 10 with modern MAUI libraries and platform integrations.
