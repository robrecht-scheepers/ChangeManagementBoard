using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
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
            var rep = new DbRepository(@"c:\TEMP\CM\dev\");
            rep.Initialize().Wait();
            var vm = new MainViewModel(rep);
            var window = new MainWindow{DataContext = vm};
            window.Show();
        }
    }
}
