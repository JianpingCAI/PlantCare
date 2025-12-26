using System.Globalization;

namespace PlantCare.App.Utils;

/// <summary>
/// Helper class for language and culture conversions
/// </summary>
public static class LanguageHelper
{
    /// <summary>
    /// Maps Language enum to culture name string
    /// </summary>
    /// <param name="language">The language enum value</param>
    /// <returns>Culture name (e.g., "zh-CN", "en", "es")</returns>
    public static string GetCultureName(Language language)
    {
        return language switch
        {
            Language.ChineseSimplified => "zh-CN",
            Language.English => "en",
            // Add more languages here as needed:
            // Language.ChineseTraditional => "zh-TW",
            // Language.Spanish => "es",
            // Language.French => "fr",
            // Language.German => "de",
            // Language.Japanese => "ja",
            // Language.Korean => "ko",
            // Language.Portuguese => "pt",
            // Language.Russian => "ru",
            // Language.Italian => "it",
            // Language.Dutch => "nl",
            // Language.Polish => "pl",
            // Language.Turkish => "tr",
            // Language.Vietnamese => "vi",
            // Language.Thai => "th",
            // Language.Indonesian => "id",
            // Language.Malay => "ms",
            // Language.Hindi => "hi",
            // Language.Arabic => "ar",
            _ => "en" // Default to English
        };
    }

    /// <summary>
    /// Gets CultureInfo from Language enum
    /// </summary>
    /// <param name="language">The language enum value</param>
    /// <returns>CultureInfo instance</returns>
    public static CultureInfo GetCultureInfo(Language language)
    {
        string cultureName = GetCultureName(language);
        return new CultureInfo(cultureName);
    }

    /// <summary>
    /// Tries to get Language enum from culture name
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "zh-CN")</param>
    /// <param name="language">Output language enum value</param>
    /// <returns>True if mapping found, false otherwise</returns>
    public static bool TryGetLanguageFromCultureName(string cultureName, out Language language)
    {
        language = cultureName?.ToLowerInvariant() switch
        {
            "zh-cn" or "zh-hans" or "zh-chs" => Language.ChineseSimplified,
            "en" or "en-us" or "en-gb" => Language.English,
            // Add more mappings here as needed
            _ => Language.English
        };

        return cultureName != null;
    }

    /// <summary>
    /// Gets all supported languages
    /// </summary>
    /// <returns>Array of supported Language enum values</returns>
    public static Language[] GetSupportedLanguages()
    {
        return new[]
        {
            Language.English,
            Language.ChineseSimplified,
            // Add more as they're implemented
        };
    }

    /// <summary>
    /// Gets display name for a language (localized if possible)
    /// </summary>
    /// <param name="language">The language enum value</param>
    /// <returns>Display name for the language</returns>
    public static string GetDisplayName(Language language)
    {
        return language switch
        {
            Language.English => "English",
            Language.ChineseSimplified => "中文(简体)",
            // Add more as needed
            _ => language.ToString()
        };
    }

    /// <summary>
    /// Gets native display name for a language
    /// </summary>
    /// <param name="language">The language enum value</param>
    /// <returns>Native display name</returns>
    public static string GetNativeDisplayName(Language language)
    {
        CultureInfo culture = GetCultureInfo(language);
        return culture.NativeName;
    }
}
