using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using fszmq;
using System.Diagnostics;

namespace chatz.client
{
  //TODO: where does user name come from?

  public partial class MainWindow :Window
  {
    public MainWindow ()
    {
      InitializeComponent ();
    }



    protected override void OnInitialized (EventArgs e)
    {
      base.OnInitialized (e);

      Task.Factory.StartNew(() => {
        using(var context = new Context()) 
        using(var client  = context.Req())
        using(var dialog  = context.Sub())
        {
          client.Connect("tcp://localhost:9001");
          dialog.Connect("tcp://localhost:9002");
          dialog.Subscribe(new []{ Encoding.UTF8.GetBytes("") });
          
          try
          { 
            while (true) 
            {
              client.Send(Encoding.UTF8.GetBytes("pblasucci\u0037Hello"));
              var ack = client.Recv();
              Debug.WriteLine(Encoding.UTF8.GetString(ack));
              
              var msg = new Byte[0][];
              if (dialog.TryGetInput(100, out msg))
                Debug.WriteLine(Encoding.UTF8.GetString(msg[0]));

              Thread.Sleep(500);
            }
          }
          catch (Exception x) 
          {
            Debug.WriteLine(x);
          }
        }
      });
    }
  }
}
