using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wpf_For_Test_File_Generator_And_Sorter.Services;

namespace Wpf_For_Test_File_Generator_And_Sorter.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    protected readonly ISettingsService SettingsService;

    protected BaseViewModel(ISettingsService settingsService)
    {
        SettingsService = settingsService;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnSettingsChanged()
    {
    }
}