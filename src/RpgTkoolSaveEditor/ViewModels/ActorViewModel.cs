using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;
using RpgTkoolSaveEditor.Model.GameDatas;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class ActorViewModel(Actor model) : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; } = model.Id;

    [ObservableProperty]
    public partial string Name { get; set; } = model.Name;

    [ObservableProperty]
    public partial int HP { get; set; } = model.HP;

    partial void OnHPChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    [ObservableProperty]
    public partial int MP { get; set; } = model.MP;

    partial void OnMPChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    [ObservableProperty]
    public partial int TP { get; set; } = model.TP;

    partial void OnTPChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    [ObservableProperty]
    public partial int Level { get; set; } = model.Level;

    partial void OnLevelChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    [ObservableProperty]
    public partial int Exp { get; set; } = model.Exp;

    partial void OnExpChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    public Actor ToModel()
    {
        return new(Id, Name, HP, MP, TP, Level, Exp);
    }
}
