using fszmq;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chatz.client
{
  public class ChatzClient
  {
    private static ConcurrentQueue<String> Pending = new ConcurrentQueue<String>();

    private Byte[] encode (String value) 
    { 
      return System.Text.Encoding.UTF8.GetBytes(value);
    }
    private String decode (Byte[] value) 
    { 
      return System.Text.Encoding.UTF8.GetString(value);
    }
    private Byte[] message (String handle, String message)
    { 
      return encode(String.Format("{0}\u221E{1}", handle, message));
    }
    private Byte[] keepAlive (String handle) 
    { 
      return encode(handle); 
    }

    public EventHandler<MessageReceivedEventArgs> MessageReceived;

    public String Handle { get; private set; }

    private void broadcast (Object state) { 
      var message = decode(state as Byte[]);
      var parts   = message.Split('\u221E');
      var args    = new MessageReceivedEventArgs(parts[0], parts[1]);

      Debug.WriteLine("[{0}] {1}", DateTime.Now, message);
      if (MessageReceived != null) MessageReceived(this, args);
    }

    private void proxyLoop (Object state) { 
      var inputs    = state as Tuple<String,TaskScheduler>;
      var handle    = inputs.Item1;
      var scheduler = inputs.Item2;

      if (String.IsNullOrWhiteSpace(handle)) 
        throw new ArgumentException("Invalid Handle", "handle");
    
      using(var context = new Context()) 
      using(var client  = context.Req())
      using(var dialog  = context.Sub())
      {
        client.Connect("tcp://localhost:9001");
        dialog.Connect("tcp://localhost:9002");
        dialog.Subscribe(new []{ encode("") });
          
        while (true)
        {
          String pending;
          switch (Pending.TryDequeue(out pending))
          {
            case true : client.Send(message(handle,pending)); break; 
            case false: client.Send(keepAlive(handle));       break;
          }          
          client.RecvAll(); // IGNORE
              
          var dispatch = new Byte[0][];
          if (dialog.TryGetInput(100, out dispatch))
          { 
            Task.Factory.StartNew (broadcast
                                  ,dispatch[0]
                                  ,Task.Factory.CancellationToken
                                  ,TaskCreationOptions.None
                                  ,scheduler);

          }

          Thread.Sleep(500);
        }
      }
    }

    public ChatzClient (String handle) { 
      this.Handle   = handle;
      var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
      var state     = Tuple.Create(handle,scheduler);

      Task.Factory.StartNew (proxyLoop, state, TaskCreationOptions.LongRunning);
    }
    
    public void Send (String message) 
    {
      Pending.Enqueue(message);
    }
  }
}
