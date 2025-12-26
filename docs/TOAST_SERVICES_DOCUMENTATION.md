# Toast Services Documentation

This document describes the toast notification services available in PlantCare.App.

---

## Overview

PlantCare.App provides two toast notification services:

| Service | Purpose | Best For |
|---------|---------|----------|
| `IToastService` | System-level toast using CommunityToolkit.Maui | Simple notifications, background alerts |
| `IInAppToastService` | Custom in-app toast with animations | Rich UI feedback, consistent cross-platform experience |

---

## 1. IToastService (System Toast)

### Description
A lightweight wrapper around CommunityToolkit.Maui's native toast functionality. Displays platform-native toast notifications.

### Interface
```csharp
public interface IToastService
{
    Task ShowAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000);
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}
```

### Implementation
- **File**: `PlantCare.App/Services/ToastService.cs`
- **Registration**: Singleton in `MauiProgram.cs`

### Characteristics
| Feature | Value |
|---------|-------|
| Appearance | Native OS toast style |
| Duration | Short (≤2000ms) or Long (>2000ms) |
| Position | Platform-controlled (usually bottom) |
| Animation | Platform-default |
| Customization | Limited |
| Works in Background | Yes (platform-dependent) |

### Usage Example
```csharp
public class MyViewModel
{
    private readonly IToastService _toastService;

    public MyViewModel(IToastService toastService)
    {
        _toastService = toastService;
    }

    public async Task DoSomething()
    {
        await _toastService.ShowAsync("Operation completed", ToastType.Success);
    }
}
```

### When to Use
- Quick, non-critical notifications
- When native look-and-feel is preferred
- Background notifications (when supported)
- Simple success/error messages

---

## 2. IInAppToastService (Custom In-App Toast)

### Description
A custom toast service that displays animated, styled toast notifications within the app. Provides consistent UI across all platforms with rich customization options.

### Interface
```csharp
public interface IInAppToastService
{
    Task ShowInfoAsync(string message, int durationMs = 3000);
    Task ShowSuccessAsync(string message, int durationMs = 3000);
    Task ShowWarningAsync(string message, int durationMs = 3000);
    Task ShowErrorAsync(string message, int durationMs = 4000);
    Task ShowAsync(string message, ToastType type, int durationMs = 3000);
    Task DismissAsync();
}
```

### Implementation
- **Service**: `PlantCare.App/Services/InAppToastService.cs`
- **View Component**: `PlantCare.App/Components/InAppToastView.xaml`
- **Registration**: Singleton in `MauiProgram.cs`

### Characteristics
| Feature | Value |
|---------|-------|
| Appearance | Custom styled card with icon |
| Duration | Precise milliseconds control |
| Position | Bottom of current page |
| Animation | Slide-in/out with fade |
| Customization | Full (colors, icons, dismiss button) |
| Works in Background | No (in-app only) |

### Toast Types and Colors
| Type | Background Color | Icon |
|------|-----------------|------|
| Info | Blue (#2196F3) | ℹ️ info |
| Success | Green (#4CAF50) | ✓ check_circle |
| Warning | Orange (#FF9800) | ⚠ warning |
| Error | Red (#F44336) | ✕ error |

### Usage Example
```csharp
public class MyViewModel
{
    private readonly IInAppToastService _inAppToastService;

    public MyViewModel(IInAppToastService inAppToastService)
    {
        _inAppToastService = inAppToastService;
    }

    public async Task SaveData()
    {
        try
        {
            // ... save operation
            await _inAppToastService.ShowSuccessAsync("Data saved successfully!");
        }
        catch (Exception ex)
        {
            await _inAppToastService.ShowErrorAsync($"Failed to save: {ex.Message}");
        }
    }

    public async Task ShowWarning()
    {
        await _inAppToastService.ShowWarningAsync("Low battery - save your work!");
    }

    public async Task ShowCustomDuration()
    {
        // Show for 5 seconds
        await _inAppToastService.ShowInfoAsync("Processing...", 5000);
    }
}
```

### When to Use
- User action feedback (save, delete, update)
- Error messages that need attention
- Consistent cross-platform appearance
- When you need precise duration control
- When you want animated transitions

---

## Comparison

| Feature | IToastService | IInAppToastService |
|---------|--------------|-------------------|
| **UI Consistency** | Varies by platform | Same on all platforms |
| **Animation** | Platform-default | Custom slide + fade |
| **Duration Control** | Short/Long only | Precise milliseconds |
| **Dismiss Button** | No | Optional |
| **Custom Icons** | No | Yes (per type) |
| **Custom Colors** | No | Yes (per type) |
| **Background Support** | Yes (limited) | No |
| **Dependencies** | CommunityToolkit.Maui | None (self-contained) |
| **Performance** | Lightweight | Slightly heavier |

---

## Architecture

### Service Registration (MauiProgram.cs)
```csharp
private static void ConfigureAppServices(MauiAppBuilder builder)
{
    // System toast (CommunityToolkit.Maui)
    builder.Services.AddSingleton<IToastService, ToastService>();
    
    // Custom in-app toast
    builder.Services.AddSingleton<IInAppToastService, InAppToastService>();
}
```

### InAppToastService Auto-Injection
The `InAppToastService` automatically:
1. Gets the current page from Shell
2. Creates an `InAppToastView` instance
3. Injects it into the page's layout
4. Handles page navigation (recreates view on new pages)
5. Falls back to system toast if injection fails

```
┌─────────────────────────────────────────────────┐
│                  Current Page                    │
│  ┌─────────────────────────────────────────┐    │
│  │              Page Content                │    │
│  │                                          │    │
│  │                                          │    │
│  │                                          │    │
│  └─────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────┐    │
│  │  ┌─────────────────────────────────┐    │    │
│  │  │ 🔵 Toast message appears here   │    │    │
│  │  └─────────────────────────────────┘    │    │
│  │         InAppToastView (auto-injected)  │    │
│  └─────────────────────────────────────────┘    │
└─────────────────────────────────────────────────┘
```

---

## Migration Guide

### From IToastService to IInAppToastService

**Before:**
```csharp
await _toastService.ShowAsync("Saved!", ToastType.Success, 2000);
```

**After:**
```csharp
await _inAppToastService.ShowSuccessAsync("Saved!", 2000);
```

### Convenience Methods
| Old Pattern | New Pattern |
|-------------|-------------|
| `ShowAsync(msg, ToastType.Info, ms)` | `ShowInfoAsync(msg, ms)` |
| `ShowAsync(msg, ToastType.Success, ms)` | `ShowSuccessAsync(msg, ms)` |
| `ShowAsync(msg, ToastType.Warning, ms)` | `ShowWarningAsync(msg, ms)` |
| `ShowAsync(msg, ToastType.Error, ms)` | `ShowErrorAsync(msg, ms)` |

---

## Best Practices

### 1. Choose the Right Service
```csharp
// Use IInAppToastService for user-facing feedback
await _inAppToastService.ShowSuccessAsync("Plant watered!");

// Use IToastService for simple/background notifications
await _toastService.ShowAsync("Sync complete", ToastType.Info);
```

### 2. Error Handling
```csharp
try
{
    await SomeOperation();
    await _inAppToastService.ShowSuccessAsync("Done!");
}
catch (Exception ex)
{
    // Use longer duration for errors so users can read them
    await _inAppToastService.ShowErrorAsync(ex.Message, 5000);
}
```

### 3. Don't Overuse Toasts
```csharp
// ❌ Bad: Toast for every small action
await _inAppToastService.ShowInfoAsync("Loading...");

// ✅ Good: Use loading overlay for loading states
IsLoading = true;
```

### 4. Keep Messages Concise
```csharp
// ❌ Bad: Too long
await _inAppToastService.ShowSuccessAsync(
    "Your plant has been successfully watered and the schedule has been updated.");

// ✅ Good: Short and clear
await _inAppToastService.ShowSuccessAsync("Plant watered!");
```

---

## Files Reference

| File | Description |
|------|-------------|
| `Services/IToastService.cs` | System toast interface + ToastType enum |
| `Services/ToastService.cs` | System toast implementation |
| `Services/IInAppToastService.cs` | In-app toast interface |
| `Services/InAppToastService.cs` | In-app toast implementation |
| `Components/InAppToastView.xaml` | Toast UI component |
| `Components/InAppToastView.xaml.cs` | Toast component code-behind |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 2024 | Initial IToastService implementation |
| 2.0 | Dec 2024 | Added IInAppToastService with custom UI |

---

**Last Updated**: December 2024  
**Phase**: Phase 3 - Advanced Polish
