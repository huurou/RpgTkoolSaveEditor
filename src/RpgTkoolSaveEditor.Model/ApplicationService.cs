using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RpgTkoolSaveEditor.Model.SaveDatas;

namespace RpgTkoolSaveEditor.Model;

public class ApplicationService(
    ISaveFileWatcher saveFileWatcher,
    [FromKeyedServices("rpgsave")] ISaveDataRepository rpgSaveDataRepository,
    [FromKeyedServices("rmmzsave")] ISaveDataRepository rmmzSaveDataRepository,
    ILogger<ApplicationService> logger
)
{
    public event EventHandler<SaveDataLoadedEventArgs>? SaveDataLoaded;

    private SaveFileType saveFileType_;
    private string? saveDirPath_;
    private CancellationTokenSource? cancellationTokenSource_;

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

        logger.LogInformation("セーブデータのセーブが要求されました。");

        var saveDataRepository = saveFileType_ switch
        {
            SaveFileType.None => null,
            SaveFileType.RpgSave => rpgSaveDataRepository,
            SaveFileType.RmmzSave => rmmzSaveDataRepository,
            _ => throw new NotSupportedException($"Unsupported save file type: {saveFileType_}")
        };
        if (saveDataRepository is null) { return; }

        cancellationTokenSource_?.Cancel();
        cancellationTokenSource_ = new();
        try
        {
            await Task.Delay(500, cancellationTokenSource_.Token);
            await saveDataRepository.SaveAsync(saveData, saveDirPath_);
            saveFileWatcher.LoadSuppressed = true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("セーブデータのセーブがキャンセルされました。");
        }
    }
}
