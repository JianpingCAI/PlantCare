# Localization Guide

This guide explains how the localization system works in PlantCare and how you can extend it. It focuses on the `LocalizationManager` helper and the .resx resource files that store the translated strings.

## 1. Overview

PlantCare relies on .NET resource files (`.resx`) for language-specific text. The `LocalizationManager` class reads those resources and exposes them to XAML bindings so every page can react to runtime language changes.

Key pieces:

- `LocalizationManager` (`PlantCare.App/Utils/LocalizationManager.cs`) – central service that loads resources, switches cultures, and notifies bindings.
- `LocalizationResources.resx` – English strings (default, neutral culture).
- `LocalizationResources.zh-CN.resx` – Simplified Chinese strings.
- `Language` enum and `LanguageProvider` – list of supported languages presented to the user.
- `SettingsViewModel` – persists the selected language and triggers culture changes.

## 2. LocalizationManager Explained

```csharp
public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();
    private readonly ResourceManager _resourceManager = new("PlantCare.App.Resources.LocalizationResources", typeof(App).Assembly);
    private CultureInfo _currentCulture = CultureInfo.CurrentCulture;

    public string? this[string key] => _resourceManager?.GetString(key, _currentCulture);

    // ... events omitted

    private void SetCulture(CultureInfo culture)
    {
        if (_currentCulture == culture)
        {
            return;
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        LanguageChanged?.Invoke(this, EventArgs.Empty);

        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage { CultureCode = culture.Name });
    }

    internal void SetLanguage(Language selectedLanguage)
    {
        switch (selectedLanguage)
        {
            case Language.English:
                SetCulture(new CultureInfo("en"));
                break;
            case Language.ChineseSimplified:
                SetCulture(new CultureInfo("zh-CN"));
                break;
        }
    }
}
```

### How it works

1. **Resource Access** – `ResourceManager` loads the compiled resource files. The indexer (`this[string key]`) fetches a value using the current culture.
2. **Culture Switching** – `SetCulture` updates `_currentCulture`, `CultureInfo.CurrentCulture`, and `CultureUICulture`. This ensures date/number formats, as well as resource lookups, obey the selected locale.
3. **UI Refresh** – `PropertyChanged` is raised twice:
   - `PropertyChanged(null)` refreshes bindings that watch all properties.
   - `PropertyChanged("Item[]")` refreshes bindings that use the indexer (e.g., `{Binding Path=[Delete], Source=...}` in XAML).
4. **Messaging** – `WeakReferenceMessenger` broadcasts `LanguageChangedMessage` so any listener (view model or service) can react to language switches.

## 3. Binding Localization Strings in XAML

Anywhere you need localized text, bind to the manager’s indexer:

```xaml
<TextBlock Text="{Binding Path=[Settings], Source={x:Static utils:LocalizationManager.Instance}}" />
```

Because the manager implements `INotifyPropertyChanged`, the control updates automatically after a language change.

## 4. Changing the Language at Runtime

The Settings page drives the language picker:

1. `SettingsViewModel.SelectedLanguage` is bound to the picker (`SettingsView.xaml`).
2. When the selection changes, the `partial void OnSelectedLanguageChanged(Language value)` method executes.
3. It calls `LocalizationManager.Instance.SetLanguage(value)` to update the culture, persists the choice via `IAppSettingsService`, and updates `App.AppLanguage`.
4. Every page receives the change because of the property-change notifications and the optional `LanguageChangedMessage`.

```csharp
partial void OnSelectedLanguageChanged(Language value)
{
    if (!_isSettingsLoaded)
    {
        return;
    }

    LocalizationManager.Instance.SetLanguage(value);
    App.UpdateAppLanguage(value);
    _ = SaveLanguagePreferenceAsync(value);
}
```

## 5. Adding a New Language

1. **Create a new `.resx` file**
   - Copy `LocalizationResources.resx` and name it `LocalizationResources.<culture>.resx` (e.g., `LocalizationResources.es.resx` for Spanish).
   - Set each string value to the translated text.

2. **Update the `Language` enum**
   - Add a new entry (e.g., `Spanish`).
   - Update `LanguageProvider` or any UI code that lists languages.

3. **Teach `LocalizationManager` about the new culture**
   - Extend `SetLanguage` with the culture name: `new CultureInfo("es")`.

4. **Run the app**
   - Select the new language in Settings. All pages that use localized bindings update immediately.

## 6. Troubleshooting Tips

- **String not translating?** Confirm that the key exists in both `LocalizationResources.resx` and the culture-specific file and that the key name matches exactly.
- **UI not updating?** Make sure you are binding to the manager’s indexer. Direct static lookups (`{x:Static strings:LocalizationResources.SomeKey}`) are resolved at compile time and will not update dynamically.
- **Date/time format still English?** Verify `CultureInfo.CurrentCulture` and `CurrentUICulture` are updated (handled inside `SetCulture`). Also ensure the values are bound via `StringFormat` that respects the culture.
- **Need to react in code?** Subscribe to `LocalizationManager.LanguageChanged` or register for `LanguageChangedMessage` with `WeakReferenceMessenger`.

## 7. Quick Reference

| Task | Steps |
| --- | --- |
| Display localized text | `{Binding Path=[Key], Source={x:Static utils:LocalizationManager.Instance}}` |
| Change language programmatically | `LocalizationManager.Instance.SetLanguage(Language.ChineseSimplified);` |
| Add a new language | Create new `.resx`, add enum entry, update `SetLanguage` |
| Listen for language changes | Subscribe to `LanguageChanged` or `WeakReferenceMessenger` |

With this flow, PlantCare delivers a responsive, user-friendly localization experience entirely on-device. Add as many languages as you need by following the template above. Happy localizing! 😊
