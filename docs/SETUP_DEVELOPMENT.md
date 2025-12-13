# PlantCare Development Setup Guide

This guide will help you set up your development environment for working on PlantCare.

## Prerequisites

### Required Software

1. **Visual Studio 2022 (v17.11 or later)** or **Visual Studio Code**
   - Visual Studio 2022 is recommended for full .NET MAUI support
   - Download: https://visualstudio.microsoft.com/

2. **.NET 10 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/10.0
   - Verify installation: `dotnet --version` should show 10.0.x

3. **.NET MAUI Workload**
   - Install via Visual Studio Installer or command line:
   ```bash
   dotnet workload install maui
   ```

### Platform-Specific Prerequisites

#### For Android Development

1. **Android SDK**
   - Minimum API Level: 21 (Android 5.0)
   - Target API Level: 34 or higher
   - Install via Visual Studio → Tools → Android → Android SDK Manager

2. **Android Emulator** (Optional but recommended)
   - Install via Visual Studio → Tools → Android → Android Device Manager
   - Recommended: Pixel 5 API 34 emulator

3. **Java Development Kit (JDK)**
   - JDK 11 or higher
   - Usually installed automatically with Visual Studio

#### For iOS Development (macOS only)

1. **Xcode** (Latest stable version)
   - Download from Mac App Store
   - Minimum version: Xcode 15

2. **Xcode Command Line Tools**
   ```bash
   xcode-select --install
   ```

3. **iOS Simulator**
   - Included with Xcode

#### For Windows Development

1. **Windows 10 SDK**
   - Version 10.0.19041.0 or higher
   - Installed via Visual Studio Installer

2. **Windows App SDK**
   - Automatically installed with .NET MAUI workload

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/JianpingCAI/PlantCare.git
cd PlantCare
```

### 2. Restore NuGet Packages

#### Using Visual Studio
- Open `PlantCare.sln`
- Right-click on Solution → Restore NuGet Packages

#### Using Command Line
```bash
dotnet restore
```

### 3. Verify Project Structure

Your solution should contain:
```
PlantCare/
├── PlantCare.App/              # Main MAUI application
├── PlantCare.Data/             # Data access layer
├── PlantCare.App.Tests/        # Unit tests
├── docs/                       # Documentation
├── screenshots/                # App screenshots
└── PlantCare.sln               # Solution file
```

### 4. Build the Solution

#### Using Visual Studio
- Select Build → Build Solution (Ctrl+Shift+B)

#### Using Command Line
```bash
dotnet build
```

Expected output: Build succeeded with 0 errors.

### 5. Run Migrations (First Time Setup)

The database will be created automatically on first run, but you can verify migrations:

```bash
cd PlantCare.Data
dotnet ef database update
```

If you encounter issues, ensure you have EF Core tools installed:
```bash
dotnet tool install --global dotnet-ef
```

## Running the Application

### Android

#### Using Visual Studio
1. Set `PlantCare.App` as startup project
2. Select framework: `net10.0-android`
3. Choose target device (emulator or physical device)
4. Press F5 or click Start Debugging

#### Using Command Line
```bash
# Run on Android emulator
dotnet build -t:Run -f net10.0-android

# Install on specific device
dotnet build -t:Run -f net10.0-android -p:AndroidDevice="<device-id>"
```

Find device ID:
```bash
adb devices
```

#### Physical Android Device Setup
1. Enable Developer Options on your device:
   - Go to Settings → About Phone
   - Tap Build Number 7 times
2. Enable USB Debugging in Developer Options
3. Connect device via USB
4. Accept USB debugging prompt on device

### iOS (macOS only)

#### Using Visual Studio for Mac
1. Set `PlantCare.App` as startup project
2. Select framework: `net10.0-ios`
3. Choose iOS Simulator or physical device
4. Press F5 or click Start Debugging

#### Using Command Line
```bash
# Run on iOS simulator
dotnet build -t:Run -f net10.0-ios
```

#### Physical iOS Device Setup
1. Enroll in Apple Developer Program (required for device deployment)
2. Configure Provisioning Profile in Xcode
3. Trust development certificate on device

### Windows

#### Using Visual Studio
1. Set `PlantCare.App` as startup project
2. Select framework: `net10.0-windows10.0.26100.0`
3. Choose target: Windows Machine
4. Press F5 or click Start Debugging

## Development Workflow

### Project Configuration

#### App Configuration Files

1. **Database**: SQLite database is created in app's data directory
   - Location: `FileSystem.AppDataDirectory/plants.db`
   - No manual configuration needed

2. **Application ID**:
   - Development: `com.jianping.myplantcare.app`
   - Configured in `PlantCare.App.csproj`

3. **Logging**:
   - Log files: `FileSystem.AppDataDirectory/app-{date}.log`
   - Configuration in `MauiProgram.cs` → `ConfigureSerilog()`

### Recommended Visual Studio Extensions

1. **XAML Styler** - Auto-format XAML files
2. **Roslynator** - C# code analysis and refactorings
3. **Git Extensions** - Enhanced Git integration
4. **.NET MAUI Hot Reload** - Included with VS 2022

### Code Style and Formatting

The project uses default C# and XAML formatting conventions:

- **Indentation**: 4 spaces
- **Line endings**: CRLF (Windows) or LF (Unix)
- **Encoding**: UTF-8
- **Naming conventions**: Follow Microsoft C# guidelines

To format code:
- Visual Studio: Ctrl+K, Ctrl+D (Format Document)
- Format on save: Tools → Options → Text Editor → C# → Code Style → Formatting

### Git Workflow

1. **Branch Naming**:
   - Features: `feature/description`
   - Bugs: `bugfix/description`
   - Hotfixes: `hotfix/description`

2. **Commit Messages**:
   - Use clear, descriptive messages
   - Start with verb (Add, Fix, Update, etc.)

3. **Pull Requests**:
   - Create PR against `master` branch
   - Ensure all tests pass
   - Update documentation if needed

## Running Tests

### Unit Tests

#### Using Visual Studio
- Test Explorer → Run All Tests (Ctrl+R, A)

#### Using Command Line
```bash
dotnet test
```

#### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~PlantServiceTests"
```

### Test Database

Tests use an in-memory SQLite database configured in test fixtures.

## Troubleshooting

### Common Issues

#### Issue: "SDK not found" error

**Solution**:
```bash
dotnet workload restore
dotnet workload install maui
```

#### Issue: Android emulator won't start

**Solution**:
1. Open Android Device Manager
2. Delete emulator
3. Create new emulator with recommended settings
4. Ensure Hyper-V is enabled (Windows) or HAXM is installed

#### Issue: Build fails with "duplicate resource" errors

**Solution**:
```bash
# Clean solution
dotnet clean
# Delete bin and obj folders
rm -rf **/bin **/obj
# Rebuild
dotnet build
```

#### Issue: Database migration errors

**Solution**:
1. Delete database file: `plants.db` from app data directory
2. Rebuild application
3. Database will be recreated on first run

#### Issue: NuGet package restore fails

**Solution**:
1. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   ```
2. Restore packages:
   ```bash
   dotnet restore --force
   ```

#### Issue: MAUI Hot Reload not working

**Solution**:
1. Enable Hot Reload: Tools → Options → Debugging → Hot Reload
2. Ensure metadata updates are enabled
3. Clean and rebuild solution

### Platform-Specific Issues

#### Android: App crashes on startup

**Check**:
1. Logcat output: `adb logcat`
2. Permissions in AndroidManifest.xml
3. Target API compatibility

#### iOS: Provisioning profile errors

**Solution**:
1. Open Xcode
2. Sign in with Apple ID
3. Create new provisioning profile
4. Select profile in Visual Studio project settings

#### Windows: App won't install

**Solution**:
1. Enable Developer Mode:
   - Settings → Update & Security → For Developers
   - Select "Developer mode"
2. Trust certificate if prompted

## Database Management

### View Database During Development

#### Android
```bash
# Pull database from device
adb pull /data/data/com.jianping.myplantcare.app/files/plants.db

# Open with SQLite browser
sqlite3 plants.db
```

#### Using DB Browser for SQLite (Recommended)
- Download: https://sqlitebrowser.org/
- Open `plants.db` file
- Browse tables and data

### Reset Database

Delete the database file from device:

#### Android
```bash
adb shell rm /data/data/com.jianping.myplantcare.app/files/plants.db
```

#### iOS Simulator
Delete app data from simulator settings or reinstall app.

## Adding New Features

### Typical Development Flow

1. **Create Database Entities** (if needed)
   - Add to `PlantCare.Data/DbModels/`
   - Update `ApplicationDbContext`
   - Create migration: `dotnet ef migrations add YourMigrationName`

2. **Create Repository** (if needed)
   - Interface in `PlantCare.Data/Repositories/interfaces/`
   - Implementation in `PlantCare.Data/Repositories/`

3. **Create Service**
   - Interface in `PlantCare.App/Services/`
   - Implementation with business logic
   - Register in `MauiProgram.cs`

4. **Create ViewModel**
   - Add to `PlantCare.App/ViewModels/`
   - Inherit from `ViewModelBase`
   - Implement MVVM pattern

5. **Create View**
   - Add XAML page in `PlantCare.App/Views/`
   - Bind to ViewModel
   - Register route in `AppShell.xaml.cs`

6. **Write Tests**
   - Add test class in `PlantCare.App.Tests/`
   - Test business logic and data access

7. **Update Documentation**
   - Update relevant .md files in `docs/`

## Performance Profiling

### Using Visual Studio Profiler

1. Debug → Performance Profiler
2. Select profiling tools:
   - CPU Usage
   - Memory Usage
   - .NET Object Allocation
3. Start profiling
4. Analyze results

### Android Profiling

Use Android Studio Profiler for detailed Android performance analysis:
1. Open Android Studio
2. File → Profile or Debug APK
3. Select installed PlantCare app
4. Analyze CPU, Memory, Network, Energy

## Deployment Builds

### Android Release Build

```bash
dotnet publish -f net10.0-android -c Release
```

Output: `.aab` file in `bin/Release/net10.0-android/publish/`

### iOS Release Build

```bash
dotnet publish -f net10.0-ios -c Release
```

### Windows Release Build

```bash
dotnet publish -f net10.0-windows10.0.26100.0 -c Release
```

## Additional Resources

### Documentation
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)

### Helpful Tools
- **Android Studio** - Android debugging and profiling
- **Xcode** - iOS development and debugging
- **Postman** - API testing (if backend added)
- **DB Browser for SQLite** - Database inspection
- **Git** - Version control

### Community
- Stack Overflow: [.NET MAUI Tag](https://stackoverflow.com/questions/tagged/.net-maui)
- GitHub Issues: [PlantCare Issues](https://github.com/JianpingCAI/PlantCare/issues)

## Getting Help

If you encounter issues not covered here:

1. Check existing GitHub issues
2. Review application logs in app data directory
3. Use Visual Studio debugger and breakpoints
4. Search .NET MAUI documentation
5. Create new GitHub issue with:
   - Description of problem
   - Steps to reproduce
   - Error messages
   - Environment details (OS, VS version, etc.)

---

**Happy Coding! 🌱**
