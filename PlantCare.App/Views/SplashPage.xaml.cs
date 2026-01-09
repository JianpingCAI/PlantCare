using System.Diagnostics;

namespace PlantCare.App.Views;

public partial class SplashPage : ContentPage
{
    private const int AnimationDuration = 800;
    private const int FeatureDelayMs = 150;
    private const int MinimumDisplayMs = 2500;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Start all animations concurrently
            await Task.WhenAll(
                AnimateLogoAsync(),
                AnimateTextElementsAsync(),
                AnimateFeaturesAsync(),
                AnimateBackgroundCirclesAsync()
            );

            // Ensure minimum display time
            await Task.Delay(MinimumDisplayMs);

            // Navigate to main app
            await NavigateToMainAppAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Animation error: {ex.Message}");
            // If animations fail, still navigate to main app
            await NavigateToMainAppAsync();
        }
    }

    private async Task AnimateLogoAsync()
    {
        try
        {
            // Initial state
            LogoBorder.Scale = 0;
            LogoBorder.Opacity = 0;
            LogoImage.Rotation = -180;

            // Animate in with bounce
            await Task.WhenAll(
                LogoBorder.ScaleToAsync(1.1, (uint)AnimationDuration, Easing.CubicOut),
                LogoBorder.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut),
                LogoImage.RotateToAsync(0, (uint)AnimationDuration, Easing.SpringOut)
            );

            // Subtle bounce back
            await LogoBorder.ScaleToAsync(1, 200, Easing.CubicOut);

            // Continuous subtle pulse animation
            _ = PulseLogoAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Logo animation error: {ex.Message}");
        }
    }

    private async Task PulseLogoAsync()
    {
        try
        {
            while (true)
            {
                await LogoBorder.ScaleToAsync(1.05, 1500, Easing.SinInOut);
                await LogoBorder.ScaleToAsync(1, 1500, Easing.SinInOut);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Pulse animation stopped: {ex.Message}");
        }
    }

    private async Task AnimateTextElementsAsync()
    {
        try
        {
            // Initial state
            AppNameLabel.TranslationY = 30;
            AppNameLabel.Opacity = 0;
            TaglineLabel.TranslationY = 30;
            TaglineLabel.Opacity = 0;
            VersionLabel.Opacity = 0;

            // Staggered fade in from bottom
            await Task.Delay(200);
            await Task.WhenAll(
                AppNameLabel.TranslateToAsync(0, 0, (uint)AnimationDuration, Easing.CubicOut),
                AppNameLabel.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut)
            );

            await Task.Delay(100);
            await Task.WhenAll(
                TaglineLabel.TranslateToAsync(0, 0, (uint)AnimationDuration, Easing.CubicOut),
                TaglineLabel.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut)
            );

            await Task.Delay(300);
            await VersionLabel.FadeToAsync(0.7, (uint)AnimationDuration, Easing.CubicOut);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Text animation error: {ex.Message}");
        }
    }

    private async Task AnimateFeaturesAsync()
    {
        try
        {
            // Initial state
            Feature1.TranslationX = -50;
            Feature1.Opacity = 0;
            Feature2.TranslationX = -50;
            Feature2.Opacity = 0;
            Feature3.TranslationX = -50;
            Feature3.Opacity = 0;

            // Staggered slide in from left
            await Task.Delay(400);

            Task<bool[]> feature1Task = Task.WhenAll(
                Feature1.TranslateToAsync(0, 0, (uint)AnimationDuration, Easing.CubicOut),
                Feature1.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut)
            );

            await Task.Delay(FeatureDelayMs);

            Task<bool[]> feature2Task = Task.WhenAll(
                Feature2.TranslateToAsync(0, 0, (uint)AnimationDuration, Easing.CubicOut),
                Feature2.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut)
            );

            await Task.Delay(FeatureDelayMs);

            Task<bool[]> feature3Task = Task.WhenAll(
                Feature3.TranslateToAsync(0, 0, (uint)AnimationDuration, Easing.CubicOut),
                Feature3.FadeToAsync(1, (uint)AnimationDuration, Easing.CubicOut)
            );

            await Task.WhenAll(feature1Task, feature2Task, feature3Task);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Features animation error: {ex.Message}");
        }
    }

    private async Task AnimateBackgroundCirclesAsync()
    {
        try
        {
            // Initial state
            Circle1.Scale = 0.5;
            Circle1.Opacity = 0;
            Circle2.Scale = 0.5;
            Circle2.Opacity = 0;

            // Animate circles growing
            await Task.WhenAll(
                Circle1.ScaleToAsync(1, 1500, Easing.CubicOut),
                Circle1.FadeToAsync(0.1, 1500, Easing.CubicOut),
                Circle2.ScaleToAsync(1, 1500, Easing.CubicOut),
                Circle2.FadeToAsync(0.1, 1500, Easing.CubicOut)
            );

            // Continuous floating animation
            _ = FloatCirclesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Background animation error: {ex.Message}");
        }
    }

    private async Task FloatCirclesAsync()
    {
        try
        {
            while (true)
            {
                await Task.WhenAll(
                    Circle1.TranslateToAsync(20, -20, 3000, Easing.SinInOut),
                    Circle2.TranslateToAsync(-20, 20, 3000, Easing.SinInOut)
                );
                await Task.WhenAll(
                    Circle1.TranslateToAsync(0, 0, 3000, Easing.SinInOut),
                    Circle2.TranslateToAsync(0, 0, 3000, Easing.SinInOut)
                );
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Float animation stopped: {ex.Message}");
        }
    }

    private async Task NavigateToMainAppAsync()
    {
        try
        {
            // Fade out animation before navigation
            await this.FadeToAsync(0, 400, Easing.CubicIn);

            // Navigate to AppShell (main app)
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SplashPage] Navigation error: {ex.Message}");
            // Fallback: direct navigation without animation
            Application.Current.MainPage = new AppShell();
        }
    }
}
