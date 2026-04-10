using Wpf_For_Test_File_Generator_And_Sorter.Models.Settings;

namespace Wpf_For_Test_File_Generator_And_Sorter.Services;

public interface ISettingsService
{
    SettingsModel SettingsModel { get; }

    Task LoadAsync();

    Task SaveAsync();
}