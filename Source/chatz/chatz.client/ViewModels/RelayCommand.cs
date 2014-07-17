using System;
using System.Diagnostics;
using System.Windows.Input;

namespace chatz.client
{
  /// <summary>
  /// A command whose sole purpose is to 
  /// relay its functionality to other
  /// objects by invoking delegates. The
  /// default return value for the CanExecute
  /// method is 'true'.
  /// </summary>
  public class RelayCommand :ICommand
  {
    private readonly Action<Object>       execute_;
    private readonly Func<Object,Boolean> canExecute_;

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand (Action<Object> execute)
      : this (execute,null)
    {
      /* PUNT */
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand (Action<Object>         execute
                        ,Func<Object, Boolean>  canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");

      execute_    = execute;
      canExecute_ = canExecute;
    }

    [DebuggerStepThrough]
    public Boolean CanExecute (Object parameters)
    {
      return canExecute_ == null ? true : canExecute_(parameters);
    }

    public event EventHandler CanExecuteChanged
    {
      add     { CommandManager.RequerySuggested += value; }
      remove  { CommandManager.RequerySuggested -= value; }
    }

    public void Execute (Object parameters)
    {
      execute_(parameters);
    }
  }
}
