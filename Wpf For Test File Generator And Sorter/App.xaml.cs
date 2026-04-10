using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf_For_Test_File_Generator_And_Sorter.Services;
using Wpf_For_Test_File_Generator_And_Sorter.Services.Generator;
using Wpf_For_Test_File_Generator_And_Sorter.Services.Settings;
using Wpf_For_Test_File_Generator_And_Sorter.Services.Sorter;
using Wpf_For_Test_File_Generator_And_Sorter.ViewModels;
using Wpf_For_Test_File_Generator_And_Sorter.Views;

namespace Wpf_For_Test_File_Generator_And_Sorter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IServiceProvider ServiceProvider { get; set; } = null!;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<IFileSorterByChunksService>(sp =>
        {
            const double dataPerThreadRatio = 0.8;
            var processorCount = Environment.ProcessorCount;
            var totalMemoryLimitInBytes = 256 * 1024 * 1024;
            var chunkSizeForThread = totalMemoryLimitInBytes / processorCount;
            var dataPerThread = (long)(chunkSizeForThread * dataPerThreadRatio);

            return new FileSorterByChunksService(dataPerThread, processorCount);
        });
        services.AddTransient<IZeroAllocationFileGeneratorService, ZeroAllocationFileGeneratorService>();

        services.AddTransient<BaseViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        services.AddTransient<FileGeneratorAndSorterView>();
        services.AddTransient<FileGeneratorAndSorterViewModel>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
            await settingsService.LoadAsync();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnStartup error: {ex}", "Information");
        }
    }
}