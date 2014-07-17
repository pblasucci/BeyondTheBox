using System.Windows;

namespace chatz.client
{
  public partial class App :Application
  {
    protected override void OnStartup (StartupEventArgs e)
    {
      //TODO: get user handle
      var model     = new ChatzClient("pblasucci");
      var viewModel = new MainWindowViewModel(model); 
      var view      = new MainWindow();
          view.DataContext = viewModel;
          view.Show();
    }
  }
}
