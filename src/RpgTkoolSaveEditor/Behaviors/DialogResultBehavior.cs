using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace RpgTkoolSaveEditor.Behaviors;

public class DialogResultBehavior : Behavior<Window>
{
    public bool? DialogResult { get => (bool?)GetValue(DialogResultProperty); set => SetValue(DialogResultProperty, value); }
    public static readonly DependencyProperty DialogResultProperty = DependencyProperty.Register(
        nameof(DialogResult), typeof(bool?), typeof(DialogResultBehavior), new PropertyMetadata(null, OnDialogResultChanged));

    private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogResultBehavior behavior)
        {
            behavior.UpdateDialogResult(e.NewValue as bool?);
        }
    }

    private void UpdateDialogResult(bool? newValue)
    {
        if (AssociatedObject?.IsVisible == true)
        {
            try
            {
                AssociatedObject.DialogResult = newValue;
            }
            catch (InvalidOperationException) { }
        }
    }
}
