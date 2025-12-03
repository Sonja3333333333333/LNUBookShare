using System;
using System.Windows.Input;

namespace LNUBookShareUI.Common
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T param)
            {
                return this._canExecute?.Invoke(param) ?? true;
            }

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T param)
            {
                this._execute.Invoke(param);
            }
        }
    }
}