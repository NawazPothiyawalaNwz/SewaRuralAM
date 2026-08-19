using System.Collections.Specialized;
using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Controls;

// Drawn directly on a Canvas via IDrawable rather than laid out with nested XAML bindings/converters,
// so rendering can't silently fail the way a data-bound Grid/BoxView bar chart can.
public class BarChartView : GraphicsView
{
    private const float RowHeight = 28;

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable<ChartItem>), typeof(BarChartView), propertyChanged: OnChanged);

    public static readonly BindableProperty BarColorProperty = BindableProperty.Create(
        nameof(BarColor), typeof(Color), typeof(BarChartView), Colors.SteelBlue, propertyChanged: OnChanged);

    public IEnumerable<ChartItem>? ItemsSource
    {
        get => (IEnumerable<ChartItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    private readonly BarChartDrawable _drawable = new();

    public BarChartView()
    {
        Drawable = _drawable;
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BarChartView view) return;

        // Bug fixed here: this used to check `items.ToList()` (always a plain List<T>, which
        // never implements INotifyCollectionChanged) instead of the actual bound collection, so
        // the CollectionChanged subscription below was dead code and the chart never updated
        // past whatever was in the ObservableCollection at initial-binding time (typically empty,
        // since that happens before the ViewModel's LoadAsync populates it).
        if (oldValue is INotifyCollectionChanged oldIncc)
            oldIncc.CollectionChanged -= view.OnCollectionChanged;

        if (newValue is INotifyCollectionChanged newIncc)
            newIncc.CollectionChanged += view.OnCollectionChanged;

        view.Refresh();
    }

    private void Refresh()
    {
        var items = ItemsSource?.ToList() ?? new List<ChartItem>();
        _drawable.Items = items;
        _drawable.BarColor = BarColor;
        HeightRequest = Math.Max(RowHeight, items.Count * RowHeight + 8);
        Invalidate();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private class BarChartDrawable : IDrawable
    {
        public List<ChartItem> Items { get; set; } = new();
        public Color BarColor { get; set; } = Colors.SteelBlue;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Items.Count == 0) return;

            var max = Math.Max(1, Items.Max(i => i.Value));
            const float labelWidth = 92;
            const float valueWidth = 30;
            const float barHeight = 10;

            var barMaxWidth = Math.Max(20, dirtyRect.Width - labelWidth - valueWidth - 8);
            float y = 4;

            foreach (var item in Items)
            {
                canvas.FontSize = 12;
                canvas.FontColor = Colors.Gray;
                canvas.DrawString(item.Label, 0, y, labelWidth, RowHeight - 4, HorizontalAlignment.Left, VerticalAlignment.Center);

                var barWidth = (float)(item.Value / (double)max) * barMaxWidth;
                var barX = labelWidth;
                var barY = y + (RowHeight - 4 - barHeight) / 2;

                canvas.FillColor = Colors.LightGray.WithAlpha(0.4f);
                canvas.FillRoundedRectangle(barX, barY, barMaxWidth, barHeight, 5);

                canvas.FillColor = BarColor;
                canvas.FillRoundedRectangle(barX, barY, Math.Max(4, barWidth), barHeight, 5);

                canvas.FontColor = Colors.Gray;
                canvas.DrawString(item.Value.ToString(), barX + barMaxWidth + 6, y, valueWidth, RowHeight - 4, HorizontalAlignment.Left, VerticalAlignment.Center);

                y += RowHeight;
            }
        }
    }
}
