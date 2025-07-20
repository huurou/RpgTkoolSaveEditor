using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RpgTkoolMvSaveEditor.Presentation.ViewModels;
using RpgTkoolSaveEditor.Model;

namespace RpgTkoolSaveEditor;

public partial class MainWindowViewModel(ApplicationService applicationService) : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = "RPGTkoolSaveDataEditor";

    [ObservableProperty]
    public partial GoldViewModel Gold { get; set; } = new(0);

    [ObservableProperty]
    public partial List<SwitchViewModel> Switches { get; set; } = [];

    [ObservableProperty]
    public partial List<VariableViewModel> Variables { get; set; } = [];

    [ObservableProperty]
    public partial List<ItemViewModel> Items { get; set; } = [];

    [ObservableProperty]
    public partial List<ItemViewModel> SelectedItems { get; set; } = [];

    [ObservableProperty]
    public partial List<WeaponViewModel> Weapons { get; set; } = [];

    [ObservableProperty]
    public partial List<WeaponViewModel> SelectedWeapons { get; set; } = [];

    [ObservableProperty]
    public partial List<ArmorViewModel> Armors { get; set; } = [];

    [ObservableProperty]
    public partial List<ArmorViewModel> SelectedArmors { get; set; } = [];

    [RelayCommand]
    public async Task LoadedAsync()
    {
        if (string.IsNullOrEmpty(SaveDirPath)) { return; }

        applicationService.SaveDataLoaded +=
            (s, e) =>
            {
                Switches = [.. e.SaveData.Switches.Select(x => new SwitchViewModel(x))];
                Variables = [.. e.SaveData.Variables.Select(x => new VariableViewModel(x))];
                Items = [.. e.SaveData.Items.Select(x => new ItemViewModel(x))];
                Weapons = [.. e.SaveData.Weapons.Select(x => new WeaponViewModel(x))];
                Armors = [.. e.SaveData.Armors.Select(x => new ArmorViewModel(x))];
                Gold.Value = e.SaveData.Gold;
            };

        WeakReferenceMessenger.Default.Register<SaveDataChangedMessage>(
            this,
            async (r, m) => await applicationService.UpdateSaveDataAsync(
                new(
                    [.. Switches.Select(x => x.ToModel())],
                    [.. Variables.Select(x => x.ToModel())],
                    Gold.Value,
                    [.. Items.Select(x => x.ToModel())],
                    [.. Weapons.Select(x => x.ToModel())],
                    [.. Armors.Select(x => x.ToModel())]
                )
            )
        );

        applicationService.Initialize(SaveDirPath);
        await applicationService.StartWatcherAsync();
    }

    [RelayCommand]
    public void Set99Items()
    {
        foreach (var item in SelectedItems)
        {
            item.Count = 99;
        }
    }

    [RelayCommand]
    public void Set99Weapons()
    {
        foreach (var weapon in SelectedWeapons)
        {
            weapon.Count = 99;
        }
    }

    [RelayCommand]
    public void Set99Armors()
    {
        foreach (var armor in SelectedArmors)
        {
            armor.Count = 99;
        }
    }

    public string? SaveDirPath { get; set; }
}
