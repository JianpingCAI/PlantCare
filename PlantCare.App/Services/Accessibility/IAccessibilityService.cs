namespace PlantCare.App.Services.Accessibility;

/// <summary>
/// Interface for accessibility features including font size adjustment and screen reader support.
/// Implements accessibility guidelines to ensure the app is usable by people with disabilities.
/// </summary>
public interface IAccessibilityService
{
    /// <summary>
    /// Gets the current font size scale factor (1.0 = default, 1.5 = 150%, etc.).
    /// </summary>
    double FontSizeScale { get; set; }

    /// <summary>
    /// Gets or sets whether screen reader features are enabled.
    /// </summary>
    bool IsScreenReaderEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether high contrast mode is enabled.
    /// </summary>
    bool IsHighContrastEnabled { get; set; }

    /// <summary>
    /// Increases the font size by a specified scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to increase (e.g., 1.2 for 20% increase).</param>
    void IncreaseFontSize(double scale);

    /// <summary>
    /// Decreases the font size by a specified scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to decrease (e.g., 0.8 for 20% decrease).</param>
    void DecreaseFontSize(double scale);

    /// <summary>
    /// Resets font size to default.
    /// </summary>
    void ResetFontSize();

    /// <summary>
    /// Announces text to screen readers for accessibility.
    /// </summary>
    /// <param name="text">The text to announce.</param>
    Task AnnounceAsync(string text);
}
