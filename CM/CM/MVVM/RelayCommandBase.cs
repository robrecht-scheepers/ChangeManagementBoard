using System;
using System.Diagnostics;

namespace CM.MVVM
{
    public abstract class RelayCommandBase<T> : CommandBase
    {
        readonly Func<T, bool> _canExecute;

        protected RelayCommandBase(Func<T, bool> canExecute)
        {
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public override bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            if (parameter == null && typeof(T).IsValueType)
            {
                return _canExecute(default(T));
            }

            return _canExecute((T)parameter);
        }
    }

    public abstract class RelayCommandBase : CommandBase
    {
        readonly Func<bool> _canExecute;

        protected RelayCommandBase(Func<bool> canExecute)
        {
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public override bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }
    }
}
