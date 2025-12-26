using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantOverviewView : ContentPageBase
{
    private bool _isFirstAppearance = true;

    public PlantOverviewView(PlantListOverviewViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Animate FAB entrance only on first appearance
        if (_isFirstAppearance && fabButton != null)
        {
            _isFirstAppearance = false;

            // Start with button invisible and scaled down
            fabButton.Opacity = 0;
            fabButton.Scale = 0.5;

            // Delay slightly to ensure layout is complete
            await Task.Delay(300);

            // Animate entrance with spring effect
            Task<bool> scaleAnimation = fabButton.ScaleToAsync(1, 400, Easing.SpringOut);
            Task<bool> fadeAnimation = fabButton.FadeToAsync(1, 300, Easing.CubicOut);

            await Task.WhenAll(scaleAnimation, fadeAnimation);
        }
    }
}
