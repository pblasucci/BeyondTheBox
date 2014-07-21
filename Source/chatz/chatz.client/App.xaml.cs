using System;
using System.Windows;

namespace chatz.client
{
  public partial class App :Application
  {
    // helper function to get handle from command-line or user-input
    private Boolean? getHandle(String[] args, out String handle)
    {
      // check command-line for a handle
      if (args.Length > 0)
      {
        var input = args[0];
        if (!String.IsNullOrWhiteSpace(input))
        {
          // handle found, return to regularly scheduled program
          handle = input;
          return true;
        }
      }
      //NOTE: temporally change shutdown mode, 
      //      so user can quit by choose not providing a handle
      Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
      
      // prompt user to enter a handle
      var editor = new EditHandleWindow();
      var result = editor.ShowDialog();
      // return user's input to main program
      handle = (result.HasValue && result.Value) ? editor.Handle : null;
      return result;
    }

    protected override void OnStartup (StartupEventArgs e)
    {
      // check for a handle on the command-line;
      // if not found, prompt user for input
      String handle;
      var result = getHandle(e.Args,out handle);
      if (result.HasValue && result.Value)
      {
        // handle acquired, pass to Model
        var chatzClient = new ChatzClient(handle);
        // reset "automatic" shuntdown mode
        Current.ShutdownMode            = ShutdownMode.OnMainWindowClose;
        // initialize View (i.e. the app's main form)
        Current.MainWindow              = new MainWindow();
        // initialize ViewModel and bind to View
        Current.MainWindow.DataContext  = new MainWindowViewModel(chatzClient);    
        // launch GUI
        Current.MainWindow.Show();
      }
      else
      {
        // no handle given, user has opted to exit application
        Current.Shutdown();
      }
    }
  }
}
