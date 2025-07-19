using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;
using RpgTkoolSaveEditor.Model.GameDatas;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class SwitchViewModel(Switch model) : ObservableObject
{
    [ObservableProperty]
    public partial bool? Value { get; set; } = model.Value;

    partial void OnValueChanged(bool? value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    public int Id { get; } = model.Id;
    public string Name { get; } = model.Name;

    public Switch ToModel()
    {
        return new(Id, Name, Value);
    }
}
