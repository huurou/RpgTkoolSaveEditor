using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;
using RpgTkoolSaveEditor.Model.GameDatas;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class VariableViewModel(Variable model) : ObservableObject
{
    [ObservableProperty]
    public partial object? Value { get; set; } = model.Value;

    partial void OnValueChanged(object? value)
    {
        if (value is not null && double.TryParse(value.ToString(), out var number))
        {
            Value = number;
        }
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    public int Id { get; } = model.Id;
    public string Name { get; } = model.Name;

    public Variable ToModel()
    {
        return new(Id, Name, Value);
    }
}
