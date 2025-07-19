namespace RpgTkoolSaveEditor.Model.GameDatas;

/// <summary>
/// アイテム情報
/// </summary>
/// <param name="Id">Id</param>
/// <param name="Name">アイテム名</param>
/// <param name="Description">説明</param>
/// <param name="Count">個数</param>
public record Item(int Id, string Name, string Description, int Count);
