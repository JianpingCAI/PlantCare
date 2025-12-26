using PlantCare.App.Services;
using System.Diagnostics;

namespace PlantCare.App.Components;

public partial class InAppToastView : ContentView
{
    private CancellationTokenSource? _dismissCts;

    public InAppToastView()
    {
        InitializeComponent();
        IsVisible = false;
        InputTransparent = true;
    }

    public async Task ShowAsync(string message, ToastType type = ToastType.Info, int? durationMs = null)
    {
        try
        {
            Debug.WriteLine($"[InAppToastView] ShowAsync called: {message}, type: {type}");
            
            // Cancel any existing dismiss timer
            _dismissCts?.Cancel();
            _dismissCts = new CancellationTokenSource();

            // Set message
            ToastMessage.Text = message;
            
            // Apply style based on type
            ApplyToastTypeStyle(type);

            int duration = durationMs ?? 3000;

            // Make visible and prepare for animation
            IsVisible = true;
            InputTransparent = false;
            
            // Reset initial state for animation
            ToastContainer.Opacity = 0;
            ToastContainer.TranslationY = 50;

            Debug.WriteLine($"[InAppToastView] Starting animation, BackgroundColor: {ToastContainer.BackgroundColor}");

            // Animate in
            await Task.WhenAll(
                ToastContainer.FadeToAsync(1, 250, Easing.CubicOut),
                ToastContainer.TranslateTo(0, 0, 250, Easing.CubicOut)
            );

            Debug.WriteLine($"[InAppToastView] Animation complete, Opacity: {ToastContainer.Opacity}");

            try
            {
                // Wait for duration
                await Task.Delay(duration, _dismissCts.Token);
                
                // Auto dismiss
                await DismissAsync();
            }
            catch (TaskCanceledException)
            {
                // Dismissed early, that's fine
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InAppToastView] Error in ShowAsync: {ex.Message}");
        }
    }

    private void ApplyToastTypeStyle(ToastType type)
    {
        Color backgroundColor;
        string iconGlyph;

        switch (type)
        {
            case ToastType.Success:
                backgroundColor = Color.FromArgb("#4CAF50");
                iconGlyph = "\ue86c"; // check_circle
                break;

            case ToastType.Error:
                backgroundColor = Color.FromArgb("#F44336");
                iconGlyph = "\ue000"; // error
                break;

            case ToastType.Warning:
                backgroundColor = Color.FromArgb("#FF9800");
                iconGlyph = "\ue002"; // warning
                break;

            case ToastType.Info:
            default:
                backgroundColor = Color.FromArgb("#2196F3");
                iconGlyph = "\ue88e"; // info
                break;
        }

        ToastContainer.BackgroundColor = backgroundColor;
        ToastIcon.Text = iconGlyph;
        
        Debug.WriteLine($"[InAppToastView] Applied style - Background: {backgroundColor}, Icon: {iconGlyph}");
    }

    public async Task DismissAsync()
    {
        try
        {
            _dismissCts?.Cancel();

            // Animate out
            await Task.WhenAll(
                ToastContainer.FadeToAsync(0, 200, Easing.CubicIn),
                ToastContainer.TranslateToAsync(0, 50, 200, Easing.CubicIn)
            );

            IsVisible = false;
            InputTransparent = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InAppToastView] Error in DismissAsync: {ex.Message}");
            IsVisible = false;
        }
    }

    private async void OnDismissTapped(object? sender, TappedEventArgs e)
    {
        await DismissAsync();
    }
}
