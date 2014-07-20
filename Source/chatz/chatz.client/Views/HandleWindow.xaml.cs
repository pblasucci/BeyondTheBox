using System;
using System.Windows;

namespace chatz.client
{
  public partial class EditHandleWindow :Window
  {
    public String Handle { get; private set; }

    public EditHandleWindow ()
    {
      InitializeComponent();
    }


    private void Okay_Click (Object sender, RoutedEventArgs e)
    {
      Handle        = null;
      DialogResult  = null;
      // validate user input and pass to caller
      if (!String.IsNullOrWhiteSpace(Input.Text))
      { 
        Handle        = Input.Text;
        DialogResult  = true;
      }
    }
    private void Cancel_Click (Object sender, RoutedEventArgs e)
    {
      // user chooses to abort app by not providing a handle
      Handle        = null;
      DialogResult  = false;
    }
  }
}
