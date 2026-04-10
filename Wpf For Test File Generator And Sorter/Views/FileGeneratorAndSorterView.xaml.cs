using System.Windows.Controls;
using Wpf_For_Test_File_Generator_And_Sorter.ViewModels;

namespace Wpf_For_Test_File_Generator_And_Sorter.Views;

public partial class FileGeneratorAndSorterView : UserControl
{
    public FileGeneratorAndSorterView(FileGeneratorAndSorterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}