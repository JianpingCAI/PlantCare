namespace PlantCare.App.Components;

public partial class SkeletonGridCard : ContentView
{
    public static readonly BindableProperty PhotoHeightProperty =
        BindableProperty.Create(nameof(PhotoHeight), typeof(double), typeof(SkeletonGridCard), 120.0);

    public static readonly BindableProperty PhotoWidthProperty =
        BindableProperty.Create(nameof(PhotoWidth), typeof(double), typeof(SkeletonGridCard), 120.0);

    public double PhotoHeight
    {
        get => (double)GetValue(PhotoHeightProperty);
        set => SetValue(PhotoHeightProperty, value);
    }

    public double PhotoWidth
    {
        get => (double)GetValue(PhotoWidthProperty);
        set => SetValue(PhotoWidthProperty, value);
    }

    public SkeletonGridCard()
    {
        InitializeComponent();
    }
}
