using System.Windows.Input;

namespace TcpChatWpfClient.Infrastructure;

// Команда для привязки к элементам интерфейса (паттерн MVVM)
public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    : ICommand
{
    public event EventHandler? CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    } // CanExecuteChanged

    public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter);

    public void Execute(object? parameter) => execute(parameter);
} // class RelayCommand
