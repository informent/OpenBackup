using System.Windows;
namespace OpenBackup;
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e) { base.OnStartup(e); ShutdownMode = ShutdownMode.OnMainWindowClose; var window = new MainWindow(); MainWindow = window; window.Show(); window.Activate(); }
}
