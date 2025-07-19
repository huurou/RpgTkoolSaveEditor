using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class GoldViewModel(int value) : ObservableObject
{
    [ObservableProperty]
    public partial int Value { get; set; } = value;

    partial void OnValueChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }
}
