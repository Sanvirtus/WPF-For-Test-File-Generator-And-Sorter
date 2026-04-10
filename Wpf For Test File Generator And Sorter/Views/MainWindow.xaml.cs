using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wpf_For_Test_File_Generator_And_Sorter.Constants;
using Wpf_For_Test_File_Generator_And_Sorter.Models.Base;
using Wpf_For_Test_File_Generator_And_Sorter.Services;
using Wpf_For_Test_File_Generator_And_Sorter.ViewModels;
using Wpf_For_Test_File_Generator_And_Sorter.Views;

namespace Wpf_For_Test_File_Generator_And_Sorter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(
        MainWindowViewModel viewModel,
        ISettingsService settingsService,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Closing += (_, _) => Application.Current.Shutdown();

        ViewModel = viewModel;
        _settingsService = settingsService;
        _serviceProvider = serviceProvider;

        DataContext = ViewModel;

        Loaded += async (s, e) => await LoadApplicationSettingsAsync();
    }

    public MainWindowViewModel ViewModel { get; set; }

    private async Task LoadApplicationSettingsAsync()
    {
        try
        {
            await _settingsService.LoadAsync();

            ApplyLanguage(_settingsService.SettingsModel.AppSettingsModel.Language);
            FillLanguageMenu();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Critical init error: {ex.Message}");
            MessageBox.Show($"Critical initialization error: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void ApplyLanguage(string cultureCode)
    {
        try
        {
            var languageDictionary = new ResourceDictionary
            {
                Source = new Uri($"Resources/Lang.{cultureCode}.xaml", UriKind.Relative)
            };

            var oldDictionary = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Lang."));

            if (oldDictionary != null)
                Application.Current.Resources.MergedDictionaries.Remove(oldDictionary);

            Application.Current.Resources.MergedDictionaries.Add(languageDictionary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Language apply error: {ex.Message}");
            MessageBox.Show($"Failed to apply language ({cultureCode}): {ex.Message}", "Language Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void FillLanguageMenu()
    {
        LanguageMenu.Items.Clear();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetName().Name + ".g.resources";

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) return;

            using (var reader = new ResourceReader(stream))
            {
                foreach (DictionaryEntry entry in reader)
                {
                    var path = entry.Key.ToString();

                    if (path.StartsWith("resources/lang.") && path.EndsWith(".baml"))
                    {
                        var cultureCode = path
                            .Replace("resources/lang.", "")
                            .Replace(".baml", "")
                            .ToUpper();

                        try
                        {
                            var culture = new CultureInfo(cultureCode);
                            var item = new MenuItem
                            {
                                Header = culture.DisplayName,
                                Tag = cultureCode
                            };

                            item.Click += ChangeLanguage_Click;
                            LanguageMenu.Items.Add(item);
                        }
                        catch
                        {
                            MessageBox.Show($"Language file for '{cultureCode}' was not found or is invalid.",
                                "Resource Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                    }
                }
            }
        }
    }

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TabItemModel tab })
        {
            ViewModel.Tabs.Remove(tab);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private async void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string cultureCode })
        {
            ApplyLanguage(cultureCode);

            _settingsService.SettingsModel.AppSettingsModel.Language = cultureCode;
            await _settingsService.SaveAsync();
        }
    }

    private void OpenGenAndSortTab_Click(object sender, RoutedEventArgs e)
    {
        var view = _serviceProvider.GetRequiredService<FileGeneratorAndSorterView>();
        ViewModel.AddTab(AppConstants.MenuGenAndSortTab, view);
    }
}