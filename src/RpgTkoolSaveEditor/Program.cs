using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RpgTkoolSaveEditor.Model;
using RpgTkoolSaveEditor.Model.SaveDatas;

namespace RpgTkoolSaveEditor;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddPathService("RpgTkoolSaveEditor");
        services.AddNLogLogging();

        services.AddSingleton<App>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ApplicationService>();
        services.AddSingleton<SaveFileWatcher>();
        services.AddKeyedSingleton<ISaveDataRepository, RpgSaveDataRepository>("rpgsave");
        services.AddKeyedSingleton<ISaveDataRepository, RmmzSaveDataRepository>("rmmzsave");

        var provider = services.BuildServiceProvider();

        var mainWindowViewModel = provider.GetRequiredService<MainWindowViewModel>();
        if (args.Length > 0)
        {
            mainWindowViewModel.SaveDirPath = args[0];
        }

        provider.RunApp();
    }

    private static void RunApp(this IServiceProvider serviceProvider)
    {
        var app = serviceProvider.GetRequiredService<App>();
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        HandleException(app, serviceProvider.GetRequiredService<ILogger<App>>());
        app.InitializeComponent();
        app.Run(mainWindow);
    }

    private static void HandleException(App app, ILogger<App> logger)
    {
        app.DispatcherUnhandledException +=
            (s, e) =>
            {
                e.Handled = true;
                logger.LogError("{ex}", e.Exception);
            };
    }
}
