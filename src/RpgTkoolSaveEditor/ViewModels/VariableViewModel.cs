using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;
using RpgTkoolSaveEditor.Model.GameDatas;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class VariableViewModel(Variable model) : ObservableObject
{
    [ObservableProperty]
    public partial object? Value { get; set; } = model.Value;

    public int Id { get; } = model.Id;
    public string Name { get; } = model.Name;

    public Variable ToModel()
    {
        return new(Id, Name, Value);
    }

    partial void OnValueChanged(object? value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }
}
