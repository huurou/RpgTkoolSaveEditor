using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace RpgTkoolSaveEditor.Model;

public static class NLogCustomExtensions
{
    public static IServiceCollection AddNLogLogging(this IServiceCollection services)
    {
        services.AddLogging(
            builder =>
            {
                builder.ClearProviders();
                var provider = services.BuildServiceProvider();
                var pathService = provider.GetRequiredService<PathService>();
                var layout = "${longdate} [${uppercase:${level:padding=-5}}] [${logger:shortName=true}] ${message}${onexception:${newline}${exception}}";
                var loggingConfig = new LoggingConfiguration();
                var logFile = new FileTarget("logFile")
                {
                    FileName = Path.Combine(pathService.LogsDir, "${shortdate}.log"),
                    Layout = layout,
                    ArchiveAboveSize = 100000000,
                };
                var logDebugger = new DebuggerTarget("logDebugger") { Layout = layout };
                var logConsole = new ColoredConsoleTarget("logConsole") { Layout = layout };
                loggingConfig.AddRuleForAllLevels(logFile);
                loggingConfig.AddRuleForAllLevels(logDebugger);
                loggingConfig.AddRuleForAllLevels(logConsole);
                builder.AddNLog(loggingConfig);
            }
        );
        return services;
    }
}
