using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolSaveEditor;
using RpgTkoolSaveEditor.Model.GameDatas;

namespace RpgTkoolMvSaveEditor.Presentation.ViewModels;

public partial class ItemViewModel(Item model) : ObservableObject
{
    [ObservableProperty]
    public partial int Count { get; set; } = model.Count;

    partial void OnCountChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new SaveDataChangedMessage());
    }

    public int Id { get; } = model.Id;
    public string Name { get; } = model.Name;
    public string Description { get; } = model.Description;

    public Item ToModel()
    {
        return new(Id, Name, Description, Count);
    }
}
