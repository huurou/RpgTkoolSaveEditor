namespace RpgTkoolSaveEditor.Model.GameDatas;

/// <summary>
/// スイッチ フラグ情報
/// </summary>
/// <param name="Id">Id スイッチ配列でのインデックス</param>
/// <param name="Name">スイッチ名</param>
/// <param name="Value">値</param>
public record Switch(int Id, string Name, bool? Value);
