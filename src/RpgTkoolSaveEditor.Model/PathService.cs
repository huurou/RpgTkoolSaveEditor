using Microsoft.Extensions.DependencyInjection;

namespace RpgTkoolSaveEditor.Model;

/// <summary>
/// 各種パス提供サービス
/// </summary>
public record PathService(IEnumerable<string> Names)
{
    /// <summary>
    /// アプリケーションフォルダパス
    /// </summary>
    public string ApplicationDir => CreateAndCombineDir([Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), .. Names]);
    /// <summary>
    /// 設定フォルダパス
    /// </summary>
    public string SettingsDir => CreateAndCombineDir([ApplicationDir, "Settings"]);
    /// <summary>
    /// settings.jsonパス
    /// </summary>
    public string SettingsJson => Path.Combine(SettingsDir, "settings.json");
    /// <summary>
    /// ログフォルダパス
    /// </summary>
    public string LogsDir => CreateAndCombineDir([ApplicationDir, "Logs"]);

    private static string CreateAndCombineDir(IEnumerable<string> paths)
    {
        var path = Path.Combine([.. paths]);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
}

public static class PathServiceExtentions
{
    public static IServiceCollection AddPathService(this IServiceCollection services, params string[] names)
    {
        services.AddSingleton<PathService>(_ => new(names));
        return services;
    }
}
