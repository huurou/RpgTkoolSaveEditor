using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RpgTkoolSaveEditor.Model.SaveDatas;

public class SaveFileWatcher(
    [FromKeyedServices("rpgsave")] ISaveDataRepository rpgSaveDataRepository,
    [FromKeyedServices("rmmzsave")] ISaveDataRepository rmmzSaveDataRepository,
    ILogger<SaveFileWatcher> logger
)
{
    private const string RPGSAVE_FILE_NAME = "file1.rpgsave";
    private const string RMMZSAVE_FILE_NAME = "file1.rmmzsave";

    public event EventHandler<SaveDataLoadedEventArgs>? SaveDataLoaded;

    public bool LoadSuppressed { get; set; }

    private FileSystemWatcher? saveDataWather_;
    private CancellationTokenSource? cancellationTokenSource_;

    public async Task<SaveFileType> StartAsync(string saveDirPath)
    {
        var saveFileType =
            File.Exists(Path.Combine(saveDirPath, RPGSAVE_FILE_NAME)) ? SaveFileType.RpgSave
            : File.Exists(Path.Combine(saveDirPath, RMMZSAVE_FILE_NAME)) ? SaveFileType.RmmzSave
            : throw new InvalidOperationException($"'{RPGSAVE_FILE_NAME}' または '{RMMZSAVE_FILE_NAME}' が '{saveDirPath}' に見つかりませんでした。");
        if (saveDataWather_?.EnableRaisingEvents == true)
        {
            saveDataWather_.EnableRaisingEvents = false;
            saveDataWather_.Dispose();
        }
        saveDataWather_ = saveFileType switch
        {
            SaveFileType.RpgSave => new(saveDirPath, RPGSAVE_FILE_NAME),
            SaveFileType.RmmzSave => new(saveDirPath, RMMZSAVE_FILE_NAME),
            _ => throw new NotSupportedException(),
        };
        saveDataWather_.Changed +=
            async (s, e) =>
            {
                logger.LogInformation("セーブデータに変更あり");

                cancellationTokenSource_?.Cancel();
                cancellationTokenSource_ = new();
                try
                {
                    await Task.Delay(100, cancellationTokenSource_.Token);
                    await LoadAsync(saveDirPath, saveFileType);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("セーブデータのロードがキャンセルされました。");
                }
            };
        saveDataWather_.EnableRaisingEvents = true;

        await LoadAsync(saveDirPath, saveFileType);

        return saveFileType;
    }

    private async Task LoadAsync(string saveDirPath, SaveFileType saveFileType)
    {
        if (LoadSuppressed)
        {
            logger.LogInformation("セーブデータのロードが抑制されました。");

            LoadSuppressed = false;
            return;
        }

        var saveDataRepository = saveFileType switch
        {
            SaveFileType.RpgSave => rpgSaveDataRepository,
            SaveFileType.RmmzSave => rmmzSaveDataRepository,
            _ => throw new NotSupportedException(),
        };
        var saveData = await saveDataRepository.LoadAsync(saveDirPath);
        SaveDataLoaded?.Invoke(this, new(saveData));
    }
}

public record SaveDataLoadedEventArgs(SaveData SaveData);
