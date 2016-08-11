using System.Windows;

namespace UpdateCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var generator = new Generator();
            generator.GetFileList();
            generator.CreateUpdateZip();
            generator.CreateUpdateXml();
        }
    }
}
