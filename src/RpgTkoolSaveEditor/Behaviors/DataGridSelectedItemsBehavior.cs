using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RpgTkoolSaveEditor.Behaviors;

public class DataGridSelectedItemsBehavior : Behavior<DataGrid>
{
    public IList SelectedItems { get => (IList)GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }
    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
        nameof(SelectedItems), typeof(IList), typeof(DataGridSelectedItemsBehavior), new FrameworkPropertyMetadata(default)
    );

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not null)
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedItems is null) { return; }
        SelectedItems.Clear();
        foreach (var item in AssociatedObject.SelectedItems)
        {
            SelectedItems.Add(item);
        }
    }
}
