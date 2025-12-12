namespace PlantCare.App.Services.Accessibility;

/// <summary>
/// Implementation of accessibility features for improved usability.
/// Provides font size scaling and screen reader support.
/// </summary>
public class AccessibilityService : IAccessibilityService
{
    private double _fontSizeScale = 1.0;
    private bool _isScreenReaderEnabled = false;
    private bool _isHighContrastEnabled = false;

    private const double MinFontScale = 0.75;
    private const double MaxFontScale = 2.0;

    public double FontSizeScale
    {
        get => _fontSizeScale;
        set => _fontSizeScale = Math.Clamp(value, MinFontScale, MaxFontScale);
    }

    public bool IsScreenReaderEnabled
    {
        get => _isScreenReaderEnabled;
        set => _isScreenReaderEnabled = value;
    }

    public bool IsHighContrastEnabled
    {
        get => _isHighContrastEnabled;
        set => _isHighContrastEnabled = value;
    }

    public void IncreaseFontSize(double scale)
    {
        FontSizeScale *= scale;
    }

    public void DecreaseFontSize(double scale)
    {
        FontSizeScale /= scale;
    }

    public void ResetFontSize()
    {
        FontSizeScale = 1.0;
    }

    public async Task AnnounceAsync(string text)
    {
        if (IsScreenReaderEnabled && !string.IsNullOrEmpty(text))
        {
            try
            {
                // Use MAUI's TextToSpeech for screen reader announcements
                await TextToSpeech.Default.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Text-to-speech failed: {ex.Message}");
            }
        }
    }
}
