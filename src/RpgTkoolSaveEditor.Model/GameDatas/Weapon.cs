namespace RpgTkoolSaveEditor.Model.GameDatas;

/// <summary>
/// 武器情報
/// </summary>
/// <param name="Id">Id</param>
/// <param name="Name">武器名</param>
/// <param name="Description">説明</param>
/// <param name="Count">個数</param>
public record Weapon(int Id, string Name, string Description, int Count);
