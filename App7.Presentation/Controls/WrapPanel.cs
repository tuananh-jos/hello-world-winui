using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace App7.Presentation.Controls;

/// <summary>
/// A simple wrap panel that arranges children left-to-right,
/// wrapping to the next row when space runs out.
/// </summary>
public class WrapPanel : Panel
{
    public double HorizontalSpacing { get; set; } = 4;
    public double VerticalSpacing { get; set; } = 4;

    protected override Size MeasureOverride(Size availableSize)
    {
        double x = 0, rowHeight = 0;
        double totalWidth = 0, totalHeight = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            var desired = child.DesiredSize;

            if (x + desired.Width > availableSize.Width && x > 0)
            {
                // Wrap to next row
                totalHeight += rowHeight + VerticalSpacing;
                x = 0;
                rowHeight = 0;
            }

            x += desired.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, desired.Height);
            totalWidth = Math.Max(totalWidth, x - HorizontalSpacing);
        }

        totalHeight += rowHeight;
        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0, y = 0, rowHeight = 0;

        foreach (UIElement child in Children)
        {
            var desired = child.DesiredSize;

            if (x + desired.Width > finalSize.Width && x > 0)
            {
                y += rowHeight + VerticalSpacing;
                x = 0;
                rowHeight = 0;
            }

            child.Arrange(new Rect(x, y, desired.Width, desired.Height));
            x += desired.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, desired.Height);
        }

        return finalSize;
    }
}
