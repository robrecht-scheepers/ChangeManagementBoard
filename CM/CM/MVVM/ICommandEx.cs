using System.Windows.Input;

namespace CM.MVVM
{
    public interface ICommandEx : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
