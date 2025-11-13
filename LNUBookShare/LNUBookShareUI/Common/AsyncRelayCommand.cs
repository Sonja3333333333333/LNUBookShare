using System.Threading.Tasks;
using System.Windows.Input;
using System;

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Predicate<T>? _canExecute;

    public AsyncRelayCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
    {
        this._execute = execute;
        this._canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return parameter is T param && (this._canExecute?.Invoke(param) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is T param)
            await this._execute(param);
    }
}
