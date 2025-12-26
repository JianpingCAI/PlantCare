namespace PlantCare.App.Components;

public partial class ShimmerView : ContentView
{
    private bool _isAnimating;
    private CancellationTokenSource? _animationCts;

    public static readonly BindableProperty ShimmerColorProperty =
        BindableProperty.Create(nameof(ShimmerColor), typeof(Color), typeof(ShimmerView), 
            Color.FromArgb("#E0E0E0"));

    public static readonly BindableProperty ShimmerHighlightColorProperty =
        BindableProperty.Create(nameof(ShimmerHighlightColor), typeof(Color), typeof(ShimmerView), 
            Color.FromArgb("#F5F5F5"));

    public static readonly BindableProperty ShimmerHeightProperty =
        BindableProperty.Create(nameof(ShimmerHeight), typeof(double), typeof(ShimmerView), 20.0);

    public static readonly BindableProperty ShimmerWidthProperty =
        BindableProperty.Create(nameof(ShimmerWidth), typeof(double), typeof(ShimmerView), -1.0);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(ShimmerView), 8.0);

    public static readonly BindableProperty IsAnimatingProperty =
        BindableProperty.Create(nameof(IsAnimating), typeof(bool), typeof(ShimmerView), true,
            propertyChanged: OnIsAnimatingChanged);

    public Color ShimmerColor
    {
        get => (Color)GetValue(ShimmerColorProperty);
        set => SetValue(ShimmerColorProperty, value);
    }

    public Color ShimmerHighlightColor
    {
        get => (Color)GetValue(ShimmerHighlightColorProperty);
        set => SetValue(ShimmerHighlightColorProperty, value);
    }

    public double ShimmerHeight
    {
        get => (double)GetValue(ShimmerHeightProperty);
        set => SetValue(ShimmerHeightProperty, value);
    }

    public double ShimmerWidth
    {
        get => (double)GetValue(ShimmerWidthProperty);
        set => SetValue(ShimmerWidthProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool IsAnimating
    {
        get => (bool)GetValue(IsAnimatingProperty);
        set => SetValue(IsAnimatingProperty, value);
    }

    public ShimmerView()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent != null && IsAnimating)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private static void OnIsAnimatingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShimmerView shimmerView)
        {
            if ((bool)newValue)
            {
                shimmerView.StartAnimation();
            }
            else
            {
                shimmerView.StopAnimation();
            }
        }
    }

    private void StartAnimation()
    {
        if (_isAnimating)
            return;

        _isAnimating = true;
        _animationCts = new CancellationTokenSource();

        _ = AnimateShimmerAsync(_animationCts.Token);
    }

    private void StopAnimation()
    {
        _isAnimating = false;
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private async Task AnimateShimmerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isAnimating)
        {
            try
            {
                // Fade to highlight color
                await ShimmerBorder.FadeTo(0.6, 600, Easing.SinInOut);
                
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Fade back to base color
                await ShimmerBorder.FadeTo(1.0, 600, Easing.SinInOut);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
