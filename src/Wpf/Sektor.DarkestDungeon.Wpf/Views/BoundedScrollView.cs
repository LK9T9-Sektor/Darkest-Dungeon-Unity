using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>A scroll viewer with a visible-row cap: it grows to the content height up to
    /// <see cref="MaxRows"/> * <see cref="RowHeight"/> and then scrolls vertically. Use it for
    /// bounded list sections (quirks, stats, resistances, diseases).</summary>
    public class BoundedScrollView : ScrollViewer
    {
        /// <summary>Identifies the <see cref="MaxRows"/> dependency property.</summary>
        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
            nameof(MaxRows),
            typeof(int),
            typeof(BoundedScrollView),
            new PropertyMetadata(1, OnBoundsChanged));

        /// <summary>Identifies the <see cref="RowHeight"/> dependency property.</summary>
        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
            nameof(RowHeight),
            typeof(double),
            typeof(BoundedScrollView),
            new PropertyMetadata(20.0, OnBoundsChanged));

        /// <summary>Identifies the <see cref="DefaultRows"/> dependency property.</summary>
        public static readonly DependencyProperty DefaultRowsProperty = DependencyProperty.Register(
            nameof(DefaultRows),
            typeof(int),
            typeof(BoundedScrollView),
            new PropertyMetadata(0, OnBoundsChanged));

        /// <summary>Initializes a new instance of the <see cref="BoundedScrollView"/> class.</summary>
        public BoundedScrollView()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            CanContentScroll = false;
            Focusable = false;
            UpdateBounds();
        }

        /// <summary>Gets or sets the maximum number of visible rows (at least one).</summary>
        public int MaxRows
        {
            get => (int)GetValue(MaxRowsProperty);
            set => SetValue(MaxRowsProperty, value);
        }

        /// <summary>Gets or sets the height of one row in device independent pixels.</summary>
        public double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        /// <summary>Gets or sets the number of rows that stay reserved even when the content is shorter.</summary>
        public int DefaultRows
        {
            get => (int)GetValue(DefaultRowsProperty);
            set => SetValue(DefaultRowsProperty, value);
        }

        private static void OnBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BoundedScrollView)d).UpdateBounds();
        }

        private void UpdateBounds()
        {
            double rowHeight = RowHeight <= 0 ? 1 : RowHeight;
            int maxRows = System.Math.Max(1, MaxRows);
            int defaultRows = System.Math.Min(System.Math.Max(0, DefaultRows), maxRows);
            MinHeight = rowHeight * defaultRows;
            MaxHeight = rowHeight * maxRows;
        }
    }
}
