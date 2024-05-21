using Microsoft.Maui.Controls.Compatibility;

namespace PlantCare.App.Layouts;

public class VariableSpanGridLayout : Layout<View>
{
    public static readonly BindableProperty ItemWidthProperty =
        BindableProperty.Create(nameof(ItemWidth), typeof(double), typeof(VariableSpanGridLayout), 100.0);

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
    {
        int columnCount = Math.Max(1, (int)(widthConstraint / ItemWidth));
        double itemWidthWithSpacing = ItemWidth + Spacing;

        double totalWidth = columnCount * itemWidthWithSpacing - Spacing;
        double totalHeight = 0;
        int rowCount = (int)Math.Ceiling((double)Children.Count / columnCount);

        foreach (var child in Children)
        {
            var size = child.Measure(ItemWidth, heightConstraint);
            totalHeight += size.Request.Height;
        }

        totalHeight = rowCount * totalHeight / Children.Count + Spacing * (rowCount - 1);

        return new SizeRequest(new Size(totalWidth, totalHeight));
    }

    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        int columnCount = Math.Max(1, (int)(width / ItemWidth));
        double itemWidthWithSpacing = ItemWidth + Spacing;
        double columnWidth = width / columnCount;

        double currentX = x;
        double currentY = y;

        int currentColumn = 0;

        foreach (var child in Children)
        {
            var size = child.Measure(ItemWidth, height);
            LayoutChildIntoBoundingRegion(child, new Rect(currentX, currentY, columnWidth, size.Request.Height));

            currentColumn++;
            if (currentColumn >= columnCount)
            {
                currentColumn = 0;
                currentX = x;
                currentY += size.Request.Height + Spacing;
            }
            else
            {
                currentX += itemWidthWithSpacing;
            }
        }
    }

    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(nameof(Spacing), typeof(double), typeof(VariableSpanGridLayout), 10.0);

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}