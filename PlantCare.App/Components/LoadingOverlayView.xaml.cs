namespace PlantCare.App.Components;

public partial class LoadingOverlayView : ContentView
{
    public static readonly BindableProperty LoadingTextProperty =
        BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(LoadingOverlayView), "Loading...");

    public static readonly BindableProperty ShowLoadingTextProperty =
        BindableProperty.Create(nameof(ShowLoadingText), typeof(bool), typeof(LoadingOverlayView), false);

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public bool ShowLoadingText
    {
        get => (bool)GetValue(ShowLoadingTextProperty);
        set => SetValue(ShowLoadingTextProperty, value);
    }

    public LoadingOverlayView()
    {
        InitializeComponent();
    }
}
