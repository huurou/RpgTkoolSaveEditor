namespace RpgTkoolSaveEditor.Model.SaveDatas;

public interface ISaveFileWatcher : IDisposable
{
    event EventHandler<SaveDataLoadedEventArgs>? SaveDataLoaded;
    bool LoadSuppressed { get; set; }

    Task<SaveFileType> StartAsync(string saveDirPath);
}
