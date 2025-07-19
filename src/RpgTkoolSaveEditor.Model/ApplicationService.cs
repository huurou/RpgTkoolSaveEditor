using Microsoft.Extensions.DependencyInjection;
using RpgTkoolSaveEditor.Model.SaveDatas;

namespace RpgTkoolSaveEditor.Model;

public class ApplicationService(
    SaveFileWatcher saveFileWatcher,
    [FromKeyedServices("rpgsave")] ISaveDataRepository rpgSaveDataRepository,
    [FromKeyedServices("rmmzsave")] ISaveDataRepository rmmzSaveDataRepository
)
{
    public event EventHandler<SaveDataLoadedEventArgs>? SaveDataLoaded;

    private SaveFileType saveFileType_;
    private string? saveDirPath_;

    public void Initialize(string saveDirPath)
    {
        saveFileWatcher.SaveDataLoaded += (s, e) => SaveDataLoaded?.Invoke(s, e);
        saveDirPath_ = saveDirPath;
    }

    public async Task StartWatcherAsync()
    {
        if (!string.IsNullOrEmpty(saveDirPath_))
        {
            saveFileType_ = await saveFileWatcher.StartAsync(saveDirPath_);
        }
    }

    public async Task UpdateSaveDataAsync(SaveData saveData)
    {
        if (string.IsNullOrEmpty(saveDirPath_)) { return; }

        var saveDataRepository = saveFileType_ switch
        {
            SaveFileType.None => null,
            SaveFileType.RpgSave => rpgSaveDataRepository,
            SaveFileType.RmmzSave => rmmzSaveDataRepository,
            _ => throw new NotSupportedException($"Unsupported save file type: {saveFileType_}")
        };
        if (saveDataRepository is not null)
        {
            await saveDataRepository.SaveAsync(saveData, saveDirPath_);
        }
    }
}
