using Microsoft.Extensions.Logging;
using RpgTkoolSaveEditor.Model.GameDatas;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RpgTkoolSaveEditor.Model.SaveDatas;

/// <summary>
/// RPGツクールMZのセーブデータリポジトリ <para/>
/// RPGツクールMZのセーブデータは
/// ①JSONをZLibで圧縮
/// ②圧縮後のバイナリをlatin1で文字列にデコード
/// ③デコードされた文字列をUTF-8でバイナリにエンコード
/// という手順で作成されている
/// </summary>
public class RmmzSaveDataRepository(ILogger<RmmzSaveDataRepository> logger) : ISaveDataRepository
{
    /// <summary>
    /// セーブデータをロードします。
    /// <param name="saveDirPath">セーブフォルダのパス</param>
    /// <returns>ロードされたセーブデータ</returns>
    public async Task<SaveData> LoadAsync(string saveDirPath)
{
    logger.LogInformation("セーブデータをロードしています。");

    try
    {
        var saveFilePath = Path.Combine(saveDirPath, "file1.rmmzsave");
        var textData = await File.ReadAllTextAsync(saveFilePath, NonBomEncoding.UTF8).ConfigureAwait(false);
        var compressedData = Encoding.GetEncoding("ISO-8859-1").GetBytes(textData);
        using var compressedStream = new MemoryStream(compressedData);
        using var zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var jsonMemoryStreamSave = new MemoryStream();
        await zlibStream.CopyToAsync(jsonMemoryStreamSave).ConfigureAwait(false);
        jsonMemoryStreamSave.Seek(0, SeekOrigin.Begin);
        if (await JsonNode.ParseAsync(jsonMemoryStreamSave).ConfigureAwait(false) is not JsonObject rootObject) { throw new InvalidOperationException($"{saveFilePath}のJSON変換に失敗しました。"); }
        if (rootObject["switches"]?["_data"] is not JsonArray switchValuesJsonArray) { throw new InvalidOperationException("swiches._dataに配列が見つかりませんでした。"); }
        if (rootObject["variables"]?["_data"] is not JsonArray variableValuesJsonArray) { throw new InvalidOperationException("variables._dataに配列が見つかりませんでした。"); }
        if (rootObject["party"]?["_gold"] is not JsonValue goldJsonValue) { throw new InvalidOperationException("party._goldに値が見つかりませんでした。"); }
        if (rootObject["party"]?["_items"] is not JsonObject heldItemsJsonObject) { throw new InvalidOperationException("party._itemsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_weapons"] is not JsonObject heldWeaponsJsonObject) { throw new InvalidOperationException("party._weaponsにオブジェクトが見つかりませんでした。"); }
        if (rootObject["party"]?["_armors"] is not JsonObject heldArmorsJsonObject) { throw new InvalidOperationException("party._armorsにオブジェクトが見つかりませんでした。"); }

        var systemFilePath = Path.Combine(saveDirPath, "..", "data", "System.json");
        using var systemFileStream = new FileStream(systemFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(systemFileStream).ConfigureAwait(false) is not JsonObject systemJsonObject) { throw new InvalidOperationException($"{systemFilePath}のJSON変換に失敗しました。"); }
        var switchNamesJsonArray = systemJsonObject["switches"]!.AsArray();
        if (systemJsonObject["variables"] is not JsonArray variableNamesJsonArray) { throw new InvalidOperationException("variablesに配列が見つかりませんでした。"); }
        var itemsFilePath = Path.Combine(saveDirPath, "..", "data", "Items.json");
        using var itemsFileStream = new FileStream(itemsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(itemsFileStream).ConfigureAwait(false) is not JsonArray itemDataJsonArray) { throw new InvalidOperationException($"{itemsFilePath}のJSON変換に失敗しました。"); }
        var weaponsFilePath = Path.Combine(saveDirPath, "..", "data", "Weapons.json");
        using var weaponsFileStream = new FileStream(weaponsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(weaponsFileStream).ConfigureAwait(false) is not JsonArray weaponDataJsonArray) { throw new InvalidOperationException($"{weaponsFilePath}のJSON変換に失敗しました。"); }
        var armorsFilePath = Path.Combine(saveDirPath, "..", "data", "Armors.json");
        using var armorsFileStream = new FileStream(armorsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (await JsonNode.ParseAsync(armorsFileStream).ConfigureAwait(false) is not JsonArray armorDataJsonArray) { throw new InvalidOperationException($"{armorsFilePath}のJSON変換に失敗しました。"); }

        var switchDataList = new List<Switch>();
        for (var i = 1; i < switchValuesJsonArray.Count; i++)
        {
            var switchName = switchNamesJsonArray[i] != null ? switchNamesJsonArray[i]!.GetValue<string>() : $"スイッチ{i:0000}";
            switchDataList.Add(new(i, switchName, switchValuesJsonArray[i]?.GetValue<bool?>()));
        }

        var variableDataList = new List<Variable>();
        for (var i = 1; i < variableValuesJsonArray.Count; i++)
        {
            var variableName = variableNamesJsonArray[i] != null ? variableNamesJsonArray[i]!.GetValue<string>() : $"変数{i:0000}";
            var value = variableValuesJsonArray[i]?.GetValueKind() switch
            {
                JsonValueKind.Number => variableValuesJsonArray[i]!.GetValue<int>(),
                JsonValueKind.String => variableValuesJsonArray[i]!.GetValue<string>(),
                _ => variableValuesJsonArray[i]?.GetValue<object>()
            };
            variableDataList.Add(new(i, variableName, value));
        }

        var heldItemDataList = new List<Item>();
        foreach (var heldItemKeyValue in heldItemsJsonObject.AsObject())
        {
            var itemId = int.Parse(heldItemKeyValue.Key);
            var itemName = itemDataJsonArray[itemId]?["name"]?.GetValue<string>() ?? $"アイテム{itemId}";
            var itemDescription = itemDataJsonArray[itemId]?["description"]?.GetValue<string>() ?? "";
            var itemCount = heldItemKeyValue.Value!.GetValue<int>();
            heldItemDataList.Add(new(itemId, itemName, itemDescription, itemCount));
        }
        heldItemDataList.Sort((x, y) => x.Id.CompareTo(y.Id));

        var heldWeaponDataList = new List<Weapon>();
        foreach (var heldWeaponKeyValue in heldWeaponsJsonObject.AsObject())
        {
            var weaponId = int.Parse(heldWeaponKeyValue.Key);
            var weaponName = weaponDataJsonArray[weaponId]?["name"]?.GetValue<string>() ?? $"武器{weaponId}";
            var weaponDescription = weaponDataJsonArray[weaponId]?["description"]?.GetValue<string>() ?? "";
            var weaponCount = heldWeaponKeyValue.Value!.GetValue<int>();
            heldWeaponDataList.Add(new(weaponId, weaponName, weaponDescription, weaponCount));
        }
        heldWeaponDataList.Sort((x, y) => x.Id.CompareTo(y.Id));

        var heldArmorDataList = new List<Armor>();
        foreach (var heldArmorKeyValue in heldArmorsJsonObject.AsObject())
        {
            var armorId = int.Parse(heldArmorKeyValue.Key);
            var armorName = armorDataJsonArray[armorId]?["name"]?.GetValue<string>() ?? $"防具{armorId}";
            var armorDescription = armorDataJsonArray[armorId]?["description"]?.GetValue<string>() ?? "";
            var armorCount = heldArmorKeyValue.Value!.GetValue<int>();
            heldArmorDataList.Add(new(armorId, armorName, armorDescription, armorCount));
        }
        heldArmorDataList.Sort((x, y) => x.Id.CompareTo(y.Id));

        logger.LogInformation("セーブデータがロードされました。");

        return new SaveData(
            switchDataList.ToImmutableList(), 
            variableDataList.ToImmutableList(), 
            goldJsonValue.GetValue<int>(), 
            heldItemDataList.ToImmutableList(), 
            heldWeaponDataList.ToImmutableList(), 
            heldArmorDataList.ToImmutableList());
    }
    catch (FileNotFoundException ex)
    {
        logger.LogError("セーブファイルまたは必要なデータファイルが見つかりません: {Message}", ex.Message);
        throw new InvalidOperationException($"必要なファイルが見つかりません: {ex.Message}", ex);
    }
    catch (DirectoryNotFoundException ex)
    {
        logger.LogError("指定されたディレクトリが見つかりません: {Message}", ex.Message);
        throw new InvalidOperationException($"ディレクトリが見つかりません: {ex.Message}", ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        logger.LogError("ファイルへのアクセスが拒否されました: {Message}", ex.Message);
        throw new InvalidOperationException($"ファイルアクセスが拒否されました: {ex.Message}", ex);
    }
    catch (IOException ex)
    {
        logger.LogError("ファイルI/Oエラーが発生しました: {Message}", ex.Message);
        throw new InvalidOperationException($"ファイルI/Oエラー: {ex.Message}", ex);
    }
    catch (JsonException ex)
    {
        logger.LogError("JSONの解析に失敗しました: {Message}", ex.Message);
        throw new InvalidOperationException($"JSONの解析エラー: {ex.Message}", ex);
    }
    catch (InvalidDataException ex)
    {
        logger.LogError("データの形式が不正です: {Message}", ex.Message);
        throw new InvalidOperationException($"データ形式エラー: {ex.Message}", ex);
    }
}

    /// <summary>
    /// セーブデータを保存します。
    /// </summary>
    /// <param name="saveData">保存するセーブデータ</param>
    /// <param name="saveDirPath">セーブフォルダのパス</param>
    public async Task SaveAsync(SaveData saveData, string saveDirPath)
{
    logger.LogInformation("セーブデータをセーブしています。");

    try
    {
        var saveFilePath = Path.Combine(saveDirPath, "file1.rmmzsave");
        var textData = await File.ReadAllTextAsync(saveFilePath, NonBomEncoding.UTF8).ConfigureAwait(false);
        var compressedData = Encoding.GetEncoding("ISO-8859-1").GetBytes(textData);
        using var compressedStream = new MemoryStream(compressedData);
        using var zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var saveJsonStream = new MemoryStream();
        await zlibStream.CopyToAsync(saveJsonStream).ConfigureAwait(false);
        saveJsonStream.Position = 0;
        if (await JsonNode.ParseAsync(saveJsonStream).ConfigureAwait(false) is not JsonObject rootObject) { throw new InvalidOperationException($"{saveFilePath}のJSON変換に失敗しました。"); }
        if (rootObject["switches"]?["_data"] is not JsonArray switchValuesJsonArray) { throw new InvalidOperationException("swiches._dataに配列が見つかりませんでした。"); }
        if (rootObject["variables"]?["_data"] is not JsonArray variableValuesJsonArray) { throw new InvalidOperationException("variables._dataに配列が見つかりませんでした。"); }
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
            variableValuesJsonArray[variable.Id] = JsonSerializer.SerializeToNode(variable.Value);
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
        await JsonSerializer.SerializeAsync(jsonMemoryStreamSave, rootObject).ConfigureAwait(false);
        jsonMemoryStreamSave.Position = 0;
        using var jsonMemoryStreamSaveReader = new StreamReader(jsonMemoryStreamSave);
        var jsonString = await jsonMemoryStreamSaveReader.ReadToEndAsync().ConfigureAwait(false);
        using (var originalStream = new MemoryStream(NonBomEncoding.UTF8.GetBytes(jsonString)))
        using (var compressedStreamSave = new MemoryStream())
        {
            using (var zlibStreamSave = new ZLibStream(compressedStreamSave, CompressionLevel.Fastest))
            {
                originalStream.CopyTo(zlibStreamSave);
            }
            compressedData = compressedStreamSave.ToArray();
        }
        textData = Encoding.GetEncoding("ISO-8859-1").GetString(compressedData);
        await File.WriteAllTextAsync(saveFilePath, textData, NonBomEncoding.UTF8).ConfigureAwait(false);

        logger.LogInformation("セーブデータがセーブされました。");
    }
    catch (FileNotFoundException ex)
    {
        logger.LogError("セーブファイルが見つかりません: {Message}", ex.Message);
        throw new InvalidOperationException($"セーブファイルが見つかりません: {ex.Message}", ex);
    }
    catch (DirectoryNotFoundException ex)
    {
        logger.LogError("指定されたディレクトリが見つかりません: {Message}", ex.Message);
        throw new InvalidOperationException($"ディレクトリが見つかりません: {ex.Message}", ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        logger.LogError("ファイルへのアクセスが拒否されました: {Message}", ex.Message);
        throw new InvalidOperationException($"ファイルアクセスが拒否されました: {ex.Message}", ex);
    }
    catch (IOException ex)
    {
        logger.LogError("ファイルI/Oエラーが発生しました: {Message}", ex.Message);
        throw new InvalidOperationException($"ファイルI/Oエラー: {ex.Message}", ex);
    }
    catch (JsonException ex)
    {
        logger.LogError("JSONの解析に失敗しました: {Message}", ex.Message);
        throw new InvalidOperationException($"JSONの解析エラー: {ex.Message}", ex);
    }
    catch (InvalidDataException ex)
    {
        logger.LogError("データの形式が不正です: {Message}", ex.Message);
        throw new InvalidOperationException($"データ形式エラー: {ex.Message}", ex);
    }
}
}

public static class NonBomEncoding
{
    public static Encoding UTF8 => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
