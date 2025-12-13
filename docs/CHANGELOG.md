# Changelog

All notable changes to PlantCare will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- iOS platform testing and optimization
- Windows platform testing and optimization
- Cloud sync functionality
- Plant identification using AI
- Weather integration for smart watering suggestions
- Social features (share plants with friends)

## [0.7.0] - 2024-XX-XX

### Added
- Care history visualization with charts
- Single plant care history view
- Improved data export/import functionality
- Image optimization for better performance
- Thumbnail generation for plant list view
- Log viewer page for debugging
- Accessibility improvements
- Security enhancements with encryption service

### Changed
- Upgraded to .NET 10
- Improved database performance with optimized queries
- Enhanced notification scheduling logic
- Better error handling throughout the app
- Updated UI with improved typography styles
- Refactored service layer for better maintainability

### Fixed
- Fixed cascade deletion issues with plant history
- Resolved notification scheduling conflicts
- Fixed image loading performance issues
- Corrected timezone issues in calendar view
- Fixed memory leaks in ViewModels

## [0.6.0] - 2024-XX-XX

### Added
- Calendar view for plant care scheduling
- Care history tracking (watering and fertilization)
- Data export to ZIP file
- Data import from ZIP file
- Multi-language support (English and Chinese)
- Settings page with notification toggles
- About page with app information

### Changed
- Migrated from individual watering/fertilization dates to history tables
- Improved database schema with separate history tracking
- Enhanced MVVM architecture with messaging pattern
- Better separation of concerns in services

### Fixed
- Database migration errors
- UI responsiveness issues
- Notification permission handling

## [0.5.0] - 2024-06-XX

### Added
- Local notification support for watering reminders
- Local notification support for fertilization reminders
- Notification permission requests
- Notification scheduling based on care frequency
- Notification tap handling to navigate to plant details

### Changed
- Improved notification service architecture
- Updated notification settings in preferences

### Fixed
- Notification scheduling timezone issues
- Duplicate notifications bug

## [0.4.0] - 2024-05-XX

### Added
- Plant fertilization tracking
- Fertilization frequency settings
- Last fertilized date tracking
- Fertilization history table
- Enhanced plant detail view with fertilization info

### Changed
- Database schema updated for fertilization support
- Updated PlantDbModel to include fertilization fields
- Improved plant add/edit form with fertilization options

### Database Migration
- Migration: `20240528164442_AddFertilize`

## [0.3.0] - 2024-05-XX

### Added
- Plant notes field for additional information
- Photo upload functionality
- Photo capture from camera
- Default plant image
- Image file management

### Changed
- Enhanced plant detail view with photos
- Improved plant add/edit form layout
- Better image handling and storage

### Fixed
- Photo path storage issues
- Camera permission handling

### Database Migration
- Migration: `20240614022629_AddNotesColumnInPlants`

## [0.2.0] - 2024-06-XX

### Added
- Watering history tracking table
- Fertilization history tracking table
- History data when creating/updating plants
- Cascade delete for history records

### Changed
- Database schema with separate history tables
- Improved data model for better history tracking

### Removed
- Single timestamp fields in favor of history tables

### Database Migrations
- Migration: `20240610015602_AddWateringAndFertilizationHistoryTables`
- Migration: `20240612020121_RemoveUnusedTable`
- Migration: `20240614034902_AddInitialRowsToWateringHistoryAndFertilizationHistory`

## [0.1.0] - 2024-05-21

### Added
- Initial release
- Plant management (Create, Read, Update, Delete)
- Plant properties: name, species, age, photo
- Watering tracking with last watered date
- Watering frequency settings
- SQLite database with Entity Framework Core
- MVVM architecture with CommunityToolkit.Mvvm
- Shell-based navigation
- Plant overview with grid layout
- Plant detail view
- Plant add/edit form
- AutoMapper for object mapping
- Dependency injection setup
- Repository pattern for data access

### Database Migration
- Migration: `20240521170322_InitialCreate`

---

## Version History

| Version | Release Date | Key Features |
|---------|-------------|--------------|
| 0.7.0 | Current | Charts, History view, .NET 10 |
| 0.6.0 | 2024-XX | Calendar, Import/Export, Localization |
| 0.5.0 | 2024-06 | Notifications |
| 0.4.0 | 2024-05 | Fertilization tracking |
| 0.3.0 | 2024-05 | Photos and Notes |
| 0.2.0 | 2024-06 | History tables |
| 0.1.0 | 2024-05 | Initial release |

## Migration Guide

### From 0.6.0 to 0.7.0

**Database**: No manual migration required. EF Core will apply migrations automatically.

**Breaking Changes**: None

**New Features**:
- Care history charts available in History tab
- New single plant history view
- Enhanced image optimization

### From 0.5.0 to 0.6.0

**Database**: Automatic migration to history-based tracking.

**Breaking Changes**: 
- Old single-date tracking replaced with history tables
- Existing plants migrated with initial history records

**Action Required**:
- No action needed - migration is automatic
- Consider exporting data before upgrading (as backup)

### From 0.4.0 to 0.5.0

**New Permissions Required**:
- Android: `POST_NOTIFICATIONS` permission
- iOS: Notification permission

**Action Required**:
- Grant notification permissions when prompted
- Configure notification settings in Settings page

---

## Deprecation Notices

### Deprecated in 0.2.0
- ❌ Single `LastWatered` field (replaced by `WateringHistory` table)
- ❌ Single `LastFertilized` field (replaced by `FertilizationHistory` table)

Note: These fields still exist in `PlantDbModel` for performance but are managed via history tables.

---

## Contributors

- **Jianping CAI** - *Initial work and ongoing development*

## Feedback and Issues

Report bugs or request features:
- GitHub Issues: https://github.com/JianpingCAI/PlantCare/issues

---

**Note**: Dates marked with `XX` are placeholders. Update with actual release dates when versions are published.
