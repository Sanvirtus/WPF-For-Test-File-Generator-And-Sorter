using System.Collections.ObjectModel;
using System.Windows.Controls;
using Wpf_For_Test_File_Generator_And_Sorter.Models.Base;
using Wpf_For_Test_File_Generator_And_Sorter.Services;

namespace Wpf_For_Test_File_Generator_And_Sorter.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private Dictionary<string, int> TabCounters { get; set; }

    public MainWindowViewModel(ISettingsService settingsService) : base(settingsService)
    {
        TabCounters = new Dictionary<string, int>();
        Tabs = new ObservableCollection<TabItemModel>();
    }

    public ObservableCollection<TabItemModel> Tabs { get; set; }

    public TabItemModel SelectedTab
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public void AddTab(string resourceKey, UserControl content)
    {
        if (!TabCounters.ContainsKey(resourceKey))
            TabCounters[resourceKey] = 1;
        else
            TabCounters[resourceKey]++;

        var newTab = new TabItemModel
        {
            HeaderKey = resourceKey,
            Index = TabCounters[resourceKey],
            Content = content
        };

        Tabs.Add(newTab);
        SelectedTab = newTab;
    }
}