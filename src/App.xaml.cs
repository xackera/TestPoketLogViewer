using System.Windows;
using TestPoketLogViewer.Services;
using TestPoketLogViewer.ViewModels;

namespace TestPoketLogViewer
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ручная сборка графа зависимостей (Composition Root)
            IJsonParserService parser = new JsonParserService();
            IFolderScannerService scanner = new FolderScannerService(parser);
            
            var viewModel = new MainViewModel(scanner);

            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };
            
            mainWindow.Show();
        }
    }
}
