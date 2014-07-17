using System;
using System.Windows;

namespace chatz.client
{
  public partial class App :Application
  {
    private Boolean? getHandle(String[] args, out String handle)
    {
      if (args.Length > 0)
      {
        var input = args[0];
        if (!String.IsNullOrWhiteSpace(input))
        {
          handle = input;
          return true;
        }
      }

      Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
      
      var editor = new EditHandleWindow();
      var result = editor.ShowDialog();
      
      handle = (result.HasValue && result.Value) ? editor.Handle : null;
      return result;
    }

    protected override void OnStartup (StartupEventArgs e)
    {
      String handle;
      var result = getHandle(e.Args,out handle);
      if (result.HasValue && result.Value)
      {
        var chatzClient = new ChatzClient(handle);
        
        Current.ShutdownMode            = ShutdownMode.OnMainWindowClose;
        Current.MainWindow              = new MainWindow();
        Current.MainWindow.DataContext  = new MainWindowViewModel(chatzClient);    
        Current.MainWindow.Show();
      }
    }
  }
}
