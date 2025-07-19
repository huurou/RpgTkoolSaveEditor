using RpgTkoolSaveEditor.Model.GameDatas;
using System.Collections.Immutable;

namespace RpgTkoolSaveEditor.Model.SaveDatas;

/// <summary>
/// セーブデータ情報
/// </summary>
/// <param name="Switches">スイッチのコレクション</param>
/// <param name="Variables">変数のコレクション</param>
/// <param name="Gold">所持金</param>
/// <param name="Items">所持アイテムのコレクション</param>
/// <param name="Weapons">所持武器のコレクション</param>
/// <param name="Armors">所持防具のコレクション</param>
public record SaveData(
    ImmutableList<Switch> Switches,
    ImmutableList<Variable> Variables,
    int Gold,
    ImmutableList<Item> Items,
    ImmutableList<Weapon> Weapons,
    ImmutableList<Armor> Armors
);
