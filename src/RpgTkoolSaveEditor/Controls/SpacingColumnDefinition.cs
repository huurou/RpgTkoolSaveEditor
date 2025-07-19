using System.Windows;
using System.Windows.Controls;

namespace RpgTkoolSaveEditor.Controls;

public class SpacingColumnDefinition : ColumnDefinition, ISpacing
{
    public double Spacing { get => Width.Value; set => Width = new GridLength(value, GridUnitType.Pixel); }

    public SpacingColumnDefinition(double width)
    {
        Width = new GridLength(width, GridUnitType.Pixel);
    }
}
