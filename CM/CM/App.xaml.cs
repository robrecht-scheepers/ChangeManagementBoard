using System;
using System.IO;
using System.Windows;

using CM.Data;

namespace CM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var rep = new DbRepository(dataDir);
            rep.Initialize().Wait();
            var vm = new MainViewModel(rep);
            var window = new MainWindow{DataContext = vm};
            window.Show();
        }
    }
}
