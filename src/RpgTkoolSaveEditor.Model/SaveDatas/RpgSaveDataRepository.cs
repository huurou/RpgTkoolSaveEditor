using LZStringCSharp;
using Microsoft.Extensions.Logging;
using RpgTkoolSaveEditor.Model.GameDatas;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RpgTkoolSaveEditor.Model.SaveDatas;

/// <summary>
/// RPGツクールMVのセーブデータリポジトリ <para/>
/// RPGツクールMVのセーブデータは
/// ①JSONをLZStringで圧縮
/// ②BASE64でエンコード
/// という手順で作成されている
/// </summary>
public class RpgSaveDataRepository(ILogger<RpgSaveDataRepository> logger) : ISaveDataRepository
{
    public async Task<SaveData> LoadAsync(string saveDirPath)
    {
        logger.LogInformation("セーブデータをロードしています。");

        var saveFilePath = Path.Combine(saveDirPath, "file1.rpgsave");
        var json = LZString.DecompressFromBase64(await File.ReadAllTextAsync(saveFilePath));
        using var jsonMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        if (await JsonNode.ParseAsync(jsonMemoryStream) is not JsonObject rootObject) { throw new InvalidOperationException($"{saveFilePath}のJSON変換に失敗しました。"); }
        if (rootObject["switches"]?["_data"]?["@a"] is not JsonArray switchValuesJsonArray) { throw new InvalidOperationException("switches._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["variables"]?["_data"]?["@a"] is not JsonArray variableValuesJsonArray) { throw new InvalidOperationException("variables._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["actors"]?["_data"]?["@a"] is not JsonArray actorsJsonArray) { throw new InvalidOperationException("actors._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["party"]?["_gold"] is not JsonValue goldJsonValue) { throw new InvalidOperationException("party._goldに値が見つかりませんでした。"); }
        if (rootObject["party"]?["_items"] is not JsonObject heldItemsJsonObject) { throw new InvalidOperationException("party._itemsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_weapons"] is not JsonObject heldWeaponsJsonObject) { throw new InvalidOperationException("party._weaponsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_armors"] is not JsonObject heldArmorsJsonObject) { throw new InvalidOperationException("party._armorsにオブジェクトが見つかりませんでした。"); }

        var systemFilePath = Path.Combine(saveDirPath, "..", "data", "System.json");
        using var systemFileStream = new FileStream(systemFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(systemFileStream) is not JsonObject systemJsonObject) { throw new InvalidOperationException($"{systemFilePath}のJSON変換に失敗しました。"); }
        var switchNamesJsonArray = systemJsonObject["switches"]!.AsArray();
        var variableNamesJsonArray = systemJsonObject["variables"]!.AsArray();
        var itemsFilePath = Path.Combine(saveDirPath, "..", "data", "Items.json");
        using var itemsFileStream = new FileStream(itemsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(itemsFileStream) is not JsonArray itemDataJsonArray) { throw new InvalidOperationException($"{itemsFilePath}のJSON変換に失敗しました。"); }
        var weaponsFilePath = Path.Combine(saveDirPath, "..", "data", "Weapons.json");
        using var weaponsFileStream = new FileStream(weaponsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(weaponsFileStream) is not JsonArray weaponDataJsonArray) { throw new InvalidOperationException($"{weaponsFilePath}のJSON変換に失敗しました。"); }
        var armorsFilePath = Path.Combine(saveDirPath, "..", "data", "Armors.json");
        using var armorsFileStream = new FileStream(armorsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(armorsFileStream) is not JsonArray armorDataJsonArray) { throw new InvalidOperationException($"{armorsFilePath}のJSON変換に失敗しました。"); }

        var switches = switchNamesJsonArray
            .Select((x, i) => (Id: i, Name: x!.GetValue<string>())).Skip(1).Where(x => !string.IsNullOrEmpty(x.Name))
            .Select(x => new Switch(x.Id, x.Name, x.Id < switchValuesJsonArray.Count ? switchValuesJsonArray[x.Id]?.GetValue<bool?>() : null));
        var variableValues = variableValuesJsonArray.Select(
            x => x?.GetValueKind() switch
            {
                JsonValueKind.String => x.GetValue<string>(),
                JsonValueKind.Number => x.GetValue<double>(),
                JsonValueKind.True or JsonValueKind.False => x.GetValue<bool>(),
                JsonValueKind.Null => null,
                // いずれにも一致しない場合は元のJsonNodeを返す
                _ => (object?)x,
            }
        ).ToList();
        var variables = variableNamesJsonArray
            .Select((x, i) => (Id: i, Name: x!.GetValue<string>())).Skip(1).Where(x => !string.IsNullOrEmpty(x.Name))
            .Select(x => new Variable(x.Id, x.Name, x.Id < variableValues.Count ? variableValues[x.Id] : null));
        var gold = goldJsonValue!.GetValue<int>();
        var items = itemDataJsonArray.OfType<JsonNode>().Where(x => !string.IsNullOrEmpty(x?["name"]?.GetValue<string>())).Select(
            x => new Item(
                x!["id"]!.GetValue<int>(),
                x["name"]!.GetValue<string>(),
                x["description"]!.GetValue<string>(),
                heldItemsJsonObject.TryGetPropertyValue(x["id"]!.GetValue<int>().ToString(), out var countJsonNode) ? countJsonNode!.GetValue<int>() : 0
            )
        );
        var weapons = weaponDataJsonArray.OfType<JsonNode>().Where(x => !string.IsNullOrEmpty(x?["name"]?.GetValue<string>())).Select(
            x => new Weapon(
                x!["id"]!.GetValue<int>(),
                x["name"]!.GetValue<string>(),
                x["description"]!.GetValue<string>(),
                heldWeaponsJsonObject.TryGetPropertyValue(x["id"]!.GetValue<int>().ToString(), out var countJsonNode) ? countJsonNode!.GetValue<int>() : 0
            )
        );
        var armors = armorDataJsonArray.OfType<JsonNode>().Where(x => !string.IsNullOrEmpty(x?["name"]?.GetValue<string>())).Select(
            x => new Armor(
                x!["id"]!.GetValue<int>(),
                x["name"]!.GetValue<string>(),
                x["description"]!.GetValue<string>(),
                heldArmorsJsonObject.TryGetPropertyValue(x["id"]!.GetValue<int>().ToString(), out var countJsonNode) ? countJsonNode!.GetValue<int>() : 0
            )
        );

        logger.LogInformation("セーブデータがロードされました。");
        return new SaveData([.. switches], [.. variables], gold, [.. items], [.. weapons], [.. armors]);
    }

    public async Task SaveAsync(SaveData saveData, string saveDirPath)
    {
        logger.LogInformation("セーブデータをセーブしています。");

        var saveFilePath = Path.Combine(saveDirPath, "file1.rpgsave");
        var json = LZString.DecompressFromBase64(await File.ReadAllTextAsync(saveFilePath));
        using var jsonMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        if (await JsonNode.ParseAsync(jsonMemoryStream) is not JsonObject rootObject) { throw new InvalidOperationException($"{saveFilePath}のJSON変換に失敗しました。"); }
        if (rootObject["switches"]?["_data"]?["@a"] is not JsonArray switchValuesJsonArray) { throw new InvalidOperationException("switches._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["variables"]?["_data"]?["@a"] is not JsonArray variableValuesJsonArray) { throw new InvalidOperationException("variables._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["actors"]?["_data"]?["@a"] is not JsonArray actorsJsonArray) { throw new InvalidOperationException("actors._data.@aに配列が見つかりませんでした。"); }
        if (rootObject["party"]?["_gold"] is not JsonValue goldJsonValue) { throw new InvalidOperationException("party._goldに値が見つかりませんでした。"); }
        if (rootObject["party"]?["_items"] is not JsonObject heldItemsJsonObject) { throw new InvalidOperationException("party._itemsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_weapons"] is not JsonObject heldWeaponsJsonObject) { throw new InvalidOperationException("party._weaponsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_armors"] is not JsonObject heldArmorsJsonObject) { throw new InvalidOperationException("party._armorsにオブジェクトが見つかりませんでした。"); }

        foreach (var @switch in saveData.Switches)
        {
            // セーブデータのスイッチ配列は要素数が全スイッチ数より少ないことがあるので足りない分だけ増やす
            while (@switch.Id >= switchValuesJsonArray.Count)
            {
                switchValuesJsonArray.Add(null);
            }
            switchValuesJsonArray[@switch.Id] = @switch.Value;
        }
        foreach (var variable in saveData.Variables)
        {
            // セーブデータの変数配列は要素数が全変数数より少ないことがあるので足りない分だけ増やす
            while (variable.Id >= variableValuesJsonArray.Count)
            {
                variableValuesJsonArray.Add(null);
            }
            variableValuesJsonArray[variable.Id] = JsonValue.Create(variable.Value);
        }
        goldJsonValue.ReplaceWith(saveData.Gold);
        foreach (var item in saveData.Items)
        {
            heldItemsJsonObject[item.Id.ToString()] = item.Count;
        }
        foreach (var weapon in saveData.Weapons)
        {
            heldWeaponsJsonObject[weapon.Id.ToString()] = weapon.Count;
        }
        foreach (var armor in saveData.Armors)
        {
            heldArmorsJsonObject[armor.Id.ToString()] = armor.Count;
        }
        using var jsonMemoryStreamSave = new MemoryStream();
        await JsonSerializer.SerializeAsync(jsonMemoryStreamSave, rootObject);
        jsonMemoryStream.Position = 0;
        using var jsonMemoryStreamReader = new StreamReader(jsonMemoryStream);
        json = await jsonMemoryStreamReader.ReadToEndAsync();
        await File.WriteAllTextAsync(saveFilePath, LZString.CompressToBase64(json));

        logger.LogInformation("セーブデータがセーブされました。");
    }
}
