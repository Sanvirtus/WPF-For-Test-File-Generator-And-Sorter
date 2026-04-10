using System.Windows;
using System.Windows.Input;

namespace Wpf_For_Test_File_Generator_And_Sorter.Services.RelayCommand;

public class AsyncRelayCommand(Func<object, Task> executeAsync, Func<object, bool>? canExecute = null)
    : ICommand
{
    private readonly Func<object, Task> _executeAsync =
        executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter)
    {
        try
        {
            await _executeAsync(parameter);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error:\n{ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}