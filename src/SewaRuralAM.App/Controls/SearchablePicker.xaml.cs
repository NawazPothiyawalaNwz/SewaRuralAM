using System.Collections;
using System.Reflection;

namespace SewaRuralAM.App.Controls;

// Deliberately self-contained: earlier versions pushed a separate modal page via
// Shell.Navigation.PushModalAsync, which turned out to be an unreliable source of "not able to
// select" / intermittent-error reports (cross-page TaskCompletionSource plumbing, back-button
// races, Shell navigation-stack interaction). This version never leaves the current page — tapping
// the field just expands a search box + list directly below it within the same ContentView, so
// there is no navigation stack, no modal lifecycle, and no cross-page state to get out of sync.
public partial class SearchablePicker : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SearchablePicker), propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(SearchablePicker), defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(SearchablePicker));

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(SearchablePicker), "Select…", propertyChanged: OnSelectedItemChanged);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(SearchablePicker), "item");

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private List<PickerRow> _allRows = new();
    private bool _suppressSelectionEvent;

    public SearchablePicker()
    {
        InitializeComponent();
        UpdateDisplay();
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SearchablePicker picker)
            picker.Close();
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SearchablePicker picker)
            picker.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var text = GetDisplayText(SelectedItem);
        if (string.IsNullOrEmpty(text))
        {
            DisplayLabel.Text = Placeholder;
            DisplayLabel.Opacity = 0.55;
        }
        else
        {
            DisplayLabel.Text = text;
            DisplayLabel.Opacity = 1;
        }
    }

    private string GetDisplayText(object? item)
    {
        if (item is null) return string.Empty;
        if (string.IsNullOrEmpty(DisplayMemberPath)) return item.ToString() ?? string.Empty;

        var property = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
    }

    private void OnHeaderTapped(object? sender, TappedEventArgs e)
    {
        if (DropdownPanel.IsVisible)
        {
            Close();
            return;
        }

        Open();
    }

    private void Open()
    {
        try
        {
            _allRows = (ItemsSource?.Cast<object>() ?? Enumerable.Empty<object>())
                .Select(item => new PickerRow { Item = item, DisplayText = GetDisplayText(item), IsSelected = Equals(item, SelectedItem) })
                .ToList();

            FilterSearchBar.Text = string.Empty;
            ItemsView.ItemsSource = _allRows;
            DropdownPanel.IsVisible = true;
            ChevronLabel.Text = IconGlyphs.ExpandLess;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SearchablePicker.Open error: {ex}");
        }
    }

    private void Close()
    {
        DropdownPanel.IsVisible = false;
        ChevronLabel.Text = IconGlyphs.ExpandMore;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim() ?? string.Empty;

        ItemsView.ItemsSource = string.IsNullOrEmpty(query)
            ? _allRows
            : _allRows.Where(r => r.DisplayText.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void OnItemSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvent) return;

        if (e.CurrentSelection.FirstOrDefault() is PickerRow row)
        {
            SelectedItem = row.Item;
            Close();

            // CollectionView keeps its own SelectedItem state; clear it so re-opening the
            // dropdown and picking the same row again still raises SelectionChanged.
            _suppressSelectionEvent = true;
            ItemsView.SelectedItem = null;
            _suppressSelectionEvent = false;
        }
    }

    private class PickerRow
    {
        public object Item { get; set; } = null!;
        public string DisplayText { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
