using System;
using System.Diagnostics;
using System.Windows.Input;

namespace chatz.client
{
  // A command that relays its functionality to other by invoking delegates;
  // Default for the CanExecute delegate returns 'true'
  public class RelayCommand :ICommand
  {
    private readonly Action<Object>       execute_;
    private readonly Func<Object,Boolean> canExecute_;
    
    // Creates a new command, which relays to the given delegates
    public RelayCommand (Action<Object>         execute
                        ,Func<Object, Boolean>  canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");

      execute_    = execute;
      canExecute_ = canExecute;
    }

    // Creates a new command that can always execute
    public RelayCommand (Action<Object> execute) : this (execute,null) { }

    // Connects command status changes to WPF CommandManager
    public event EventHandler CanExecuteChanged
    {
      add     { CommandManager.RequerySuggested += value; }
      remove  { CommandManager.RequerySuggested -= value; }
    }

    // If provided, Defer to delegate; otherwise return 'true'
    [DebuggerStepThrough]
    public Boolean CanExecute (Object parameters)
    {
      return canExecute_ == null ? true : canExecute_(parameters);
    }

    // Punt to delegate
    public void Execute (Object parameters) { execute_(parameters); }
  }
}
 