using System;
using System.Threading.Tasks;

namespace CM.MVVM
{
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking asynchronous delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class AsyncRelayCommand<T> : RelayCommandBase<T>, IAsyncCommand
    {
        private readonly Func<T, Task> _execute;
        private Task _execution;

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
            : base(canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _execution = Task.FromResult(0);
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!_execution.IsCompleted) return;

            _execution = _execute((T)parameter);
            await _execution;
        }
    }

    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking asynchronous delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class AsyncRelayCommand : RelayCommandBase, IAsyncCommand
    {
        private readonly Func<Task> _execute;
        private Task _execution;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null) : base(canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _execution = Task.FromResult(0);
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!_execution.IsCompleted) return;

            _execution = _execute();
            await _execution;
        }
    }
}
