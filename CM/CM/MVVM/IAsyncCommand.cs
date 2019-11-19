using System.Threading.Tasks;

namespace CM.MVVM
{
    public interface IAsyncCommand : ICommandEx
    {
        Task ExecuteAsync(object parameter);
    }
}
