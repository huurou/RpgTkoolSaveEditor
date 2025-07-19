namespace RpgTkoolSaveEditor.Model.SaveDatas;

/// <summary>
/// セーブデータのリポジトリインターフェース
/// </summary>
public interface ISaveDataRepository
{
    /// <summary>
    /// セーブデータをロードします。
    /// <param name="saveDirPath">セーブフォルダのパス</param>
    /// <returns>ロードされたセーブデータ</returns>
    Task<SaveData> LoadAsync(string saveDirPath);

    /// <summary>
    /// セーブデータを保存します。
    /// </summary>
    /// <param name="saveData">保存するセーブデータ</param>
    /// <param name="saveDirPath">セーブフォルダのパス</param>
    public Task SaveAsync(SaveData saveData, string saveDirPath);
}
