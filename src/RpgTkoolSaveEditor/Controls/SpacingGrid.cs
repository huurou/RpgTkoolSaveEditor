using System.Windows;
using System.Windows.Controls;

namespace RpgTkoolSaveEditor.Controls;

public class SpacingGrid : Grid
{
    public double RowSpacing { get => (double)GetValue(RowSpacingProperty); set => SetValue(RowSpacingProperty, value); }

    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing), typeof(double), typeof(SpacingGrid), new FrameworkPropertyMetadata(default));

    public double ColumnSpacing { get => (double)GetValue(ColumnSpacingProperty); set => SetValue(ColumnSpacingProperty, value); }

    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing), typeof(double), typeof(SpacingGrid), new FrameworkPropertyMetadata(default));

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        UpdateRows();
        UpdateColumns();
        if (visualAdded is UIElement element)
        {
            element.IsVisibleChanged += OnElementIsVisibleChanged;
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.Property.Name)
        {
            case nameof(RowSpacing):
                foreach (var spacingRow in RowDefinitions.OfType<ISpacing>())
                {
                    spacingRow.Spacing = RowSpacing;
                }
                break;

            case nameof(ColumnSpacing):
                foreach (var spacingColumn in ColumnDefinitions.OfType<ISpacing>())
                {
                    spacingColumn.Spacing = ColumnSpacing;
                }
                break;

            default: break;
        }
    }

    private void UpdateRows()
    {
        var itemRowDefinitions = RowDefinitions.Where(x => x is not ISpacing).ToList();
        var actualRowDefinitions = new List<RowDefinition>();
        var itemRowDefinitionIndex = 0;
        for (var i = 0; itemRowDefinitionIndex < itemRowDefinitions.Count; i++)
        {
            // 偶数番目の行はアイテム用の行
            if (i % 2 == 0)
            {
                actualRowDefinitions.Add(itemRowDefinitions[itemRowDefinitionIndex]);
                itemRowDefinitionIndex++;
            }
            // 奇数番目の行はスペース用の行
            else
            {
                actualRowDefinitions.Add(new SpacingRowDefinition(RowSpacing));
            }
        }
        RowDefinitions.Clear();
        foreach (var rowDefinition in actualRowDefinitions)
        {
            RowDefinitions.Add(rowDefinition);
        }
    }

    private void UpdateColumns()
    {
        var itemColumnDefinitions = ColumnDefinitions.Where(x => x is not ISpacing).ToList();
        var actualColumnDefinitions = new List<ColumnDefinition>();
        var itemColumnDefinitionIndex = 0;
        for (var i = 0; itemColumnDefinitionIndex < itemColumnDefinitions.Count; i++)
        {
            // 偶数番目の列はアイテム用の列
            if (i % 2 == 0)
            {
                actualColumnDefinitions.Add(itemColumnDefinitions[itemColumnDefinitionIndex]);
                itemColumnDefinitionIndex++;
            }
            // 奇数番目の列はスペース用の列
            else
            {
                actualColumnDefinitions.Add(new SpacingColumnDefinition(ColumnSpacing));
            }
        }
        ColumnDefinitions.Clear();
        foreach (var columnDefinition in actualColumnDefinitions)
        {
            ColumnDefinitions.Add(columnDefinition);
        }
    }

    private void OnElementIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is UIElement element)
        {
            SetRow(element, GetRow(element) * 2);
            SetRowSpan(element, GetRowSpan(element) * 2 - 1);
            SetColumn(element, GetColumn(element) * 2);
            SetColumnSpan(element, GetColumnSpan(element) * 2 - 1);

            element.IsVisibleChanged -= OnElementIsVisibleChanged;
        }
    }
}
