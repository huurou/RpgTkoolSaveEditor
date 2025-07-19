namespace RpgTkoolSaveEditor.Model.GameDatas;

/// <summary>
/// 防具情報
/// </summary>
/// <param name="Id">Id</param>
/// <param name="Name">防具名</param>
/// <param name="Description">説明</param>
/// <param name="Count">個数</param>
public record Armor(int Id, string Name, string Description, int Count);
