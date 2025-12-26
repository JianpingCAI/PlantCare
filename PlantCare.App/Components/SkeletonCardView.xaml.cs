namespace PlantCare.App.Components;

public partial class SkeletonCardView : ContentView
{
    public static readonly BindableProperty ImageHeightProperty =
        BindableProperty.Create(nameof(ImageHeight), typeof(double), typeof(SkeletonCardView), 120.0);

    public static readonly BindableProperty ImageWidthProperty =
        BindableProperty.Create(nameof(ImageWidth), typeof(double), typeof(SkeletonCardView), 120.0);

    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public SkeletonCardView()
    {
        InitializeComponent();
    }
}
