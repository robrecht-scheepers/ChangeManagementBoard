using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CM.MVVM
{
    public abstract class CommandBase : ICommandEx
    {
        private readonly Dispatcher _dispatcher;

        public event EventHandler CanExecuteChanged;

        protected CommandBase()
        {
            _dispatcher = Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;
            CommandManager.RequerySuggested += CommandManager_RequerySuggested;
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public void RaiseCanExecuteChanged()
        {
            if (_dispatcher.CheckAccess())
                OnCanExecuteChanged();
            else
                _dispatcher.BeginInvoke(new Action(OnCanExecuteChanged));
        }

        private void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
