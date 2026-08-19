using System.Collections.Specialized;
using SewaRuralAM.App.ViewModels;

namespace SewaRuralAM.App.Controls;

// Drawn directly on a Canvas via IDrawable rather than laid out with nested XAML bindings/converters,
// so rendering can't silently fail the way a data-bound Grid/BoxView bar chart can.
//
// Labels are stacked above their bar (rather than squeezed into a fixed-width side column) because
// this chart is used for full location breadcrumb chains (e.g. "Head Office > Building A > Floor 1 >
// Room 101 > Rack 1 > Shelf A"), which are far too long for a single narrow column. Label text wraps
// to up to MaxLabelLines using a character-count estimate rather than exact glyph measurement — good
// enough for readable wrapping without depending on ICanvas text-measurement APIs.
public class BarChartView : GraphicsView
{
    private const float LabelLineHeight = 16;
    private const float LabelFontSize = 12;
    private const float BarHeight = 12;
    private const float BarRowHeight = 22;
    private const float RowSpacing = 14;
    private const float AvgCharWidth = 6.2f;
    private const int MaxLabelLines = 3;

    private static readonly Color LabelColor = Color.FromArgb("#544138");
    private static readonly Color ValueColor = Color.FromArgb("#2A1E19");
    private static readonly Color TrackColor = Color.FromArgb("#E5DAD1");

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
        SizeChanged += (_, _) => Refresh();
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

        // Width isn't known yet before the first layout pass; fall back to a reasonable estimate
        // and let the SizeChanged handler re-run this once the real width is available.
        var availableWidth = Width > 0 ? (float)Width - 4 : 260f;
        var maxChars = Math.Max(12, (int)(availableWidth / AvgCharWidth));

        var rows = items.Select(item =>
        {
            var lines = WrapText(item.Label, maxChars, MaxLabelLines);
            var height = lines.Count * LabelLineHeight + BarRowHeight + RowSpacing;
            return new ChartRow(item, lines, height);
        }).ToList();

        _drawable.Rows = rows;
        _drawable.BarColor = BarColor;
        HeightRequest = Math.Max(BarRowHeight, rows.Sum(r => r.Height) + 4);
        Invalidate();
    }

    private static List<string> WrapText(string text, int maxChars, int maxLines)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var current = string.Empty;

        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (candidate.Length > maxChars && current.Length > 0)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0) lines.Add(current);
        if (lines.Count == 0) lines.Add(text);

        if (lines.Count <= maxLines) return lines;

        var truncated = lines.Take(maxLines).ToList();
        var last = truncated[^1];
        truncated[^1] = last.Length > 1 ? last[..^1].TrimEnd() + "…" : last + "…";
        return truncated;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    private record ChartRow(ChartItem Item, List<string> Lines, float Height);

    private class BarChartDrawable : IDrawable
    {
        public List<ChartRow> Rows { get; set; } = new();
        public Color BarColor { get; set; } = Colors.SteelBlue;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Rows.Count == 0) return;

            var max = Math.Max(1, Rows.Max(r => r.Item.Value));
            var barMaxWidth = Math.Max(20, dirtyRect.Width - 44);
            float y = 2;

            foreach (var row in Rows)
            {
                canvas.FontSize = LabelFontSize;
                canvas.FontColor = LabelColor;
                foreach (var line in row.Lines)
                {
                    canvas.DrawString(line, 0, y, dirtyRect.Width, LabelLineHeight, HorizontalAlignment.Left, VerticalAlignment.Center);
                    y += LabelLineHeight;
                }

                var barWidth = (float)(row.Item.Value / (double)max) * barMaxWidth;
                var barY = y + (BarRowHeight - BarHeight) / 2;

                canvas.FillColor = TrackColor;
                canvas.FillRoundedRectangle(0, barY, barMaxWidth, BarHeight, BarHeight / 2);

                canvas.FillColor = BarColor;
                canvas.FillRoundedRectangle(0, barY, Math.Max(6, barWidth), BarHeight, BarHeight / 2);

                canvas.FontSize = LabelFontSize;
                canvas.FontColor = ValueColor;
                canvas.DrawString(row.Item.Value.ToString(), barMaxWidth + 6, y, 38, BarRowHeight, HorizontalAlignment.Left, VerticalAlignment.Center);

                y += BarRowHeight + RowSpacing;
            }
        }
    }
}
