namespace RpgTkoolSaveEditor.Model.GameDatas;

/// <summary>
/// 変数情報
/// </summary>
/// <param name="Id">Id 変数配列でのインデックス</param>
/// <param name="Name">変数名</param>
/// <param name="Value">値</param>
public record Variable(int Id, string Name, object? Value);
