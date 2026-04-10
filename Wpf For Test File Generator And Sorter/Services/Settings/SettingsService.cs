using System.IO;
using System.Text.Json;
using Wpf_For_Test_File_Generator_And_Sorter.Constants;
using Wpf_For_Test_File_Generator_And_Sorter.Models.Settings;

namespace Wpf_For_Test_File_Generator_And_Sorter.Services.Settings;

public class SettingsService : ISettingsService
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public SettingsModel SettingsModel { get; private set; } = new();

    public async Task LoadAsync()
    {
        if (!File.Exists(AppConstants.SettingsFile))
        {
            SettingsModel = new SettingsModel();
            await SaveAsync();
            return;
        }

        try
        {
            await using var openStream = File.OpenRead(AppConstants.SettingsFile);
            var loadedSettings = await JsonSerializer.DeserializeAsync<SettingsModel>(openStream, _options);

            SettingsModel = loadedSettings ?? new SettingsModel();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            SettingsModel = new SettingsModel();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await using var createStream = File.Create(AppConstants.SettingsFile);
            await JsonSerializer.SerializeAsync(createStream, SettingsModel, _options);

            await createStream.FlushAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            throw;
        }
    }
}