using fszmq;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace chatz.client
{
  // Proxy to chat server; maintains ZMQ connection on a background task
  public class ChatzClient
  { 
    // Thread-safe queue used to stage messages for transmission to server
    private static ConcurrentQueue<String> Pending = new ConcurrentQueue<String>();

    // Event triggered when a broadcast is received from the server
    public EventHandler<MessageReceivedEventArgs> MessageReceived;

    // Handle with which messages are tagged before transmission
    public String Handle { get; private set; }

    public ChatzClient (String handle) { 
      // keep track of this client's handle
      this.Handle = handle;
      // get sync point for ensuring events are raised on main thread
      var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
      // assemble state for background task utilization
      var state = Tuple.Create(handle,scheduler);
      // start long-running background task
      Task.Factory.StartNew (proxyLoop, state, TaskCreationOptions.LongRunning);
    }
    
    // Enqueues a new message for eventual transmission to the server
    public void Send (String message) { Pending.Enqueue(message); }

    
    // helper method manages connection to server and transmission of messages
    // NOTE: executed as long-running background task (see: constructor)
    private void proxyLoop (Object state) { 
      // get initial state (from main thread)
      var inputs    = state as Tuple<String,TaskScheduler>;
      var handle    = inputs.Item1;
      var scheduler = inputs.Item2;

      // minimal input validation
      if (String.IsNullOrWhiteSpace(handle)) 
        throw new ArgumentException("Invalid Handle", "handle");
      
      // initialize connection to server
      using(var context = new Context()) 
      using(var client  = context.Req())
      using(var dialog  = context.Sub())
      {
        client.Connect("tcp://localhost:9001"); // socket for commands
        dialog.Connect("tcp://localhost:9002"); // socket for broadcasts
        dialog.Subscribe(new []{ encode("") }); // get to all broadcast topics
          
        while (true)
        {
          // check queue for outgoing message
          String pending; 
          switch (Pending.TryDequeue(out pending))
          {
            // message in queue, package and send to server
            case true: 
              client.Send(message(handle,pending)); 
              break; 
            // no mesage from GUI (idle), ping server
            case false: 
              client.Send(keepAlive(handle)); 
              break;
          }
          // NOTE: server replies to commands with list of connected clients
          client.RecvAll(); // IGNORE
              
          // poll socket for broadcast from server
          var dispatch = new Byte[0][]; // get
          if (dialog.TryGetInput(500, out dispatch))
          { 
            // broadcast received, trigger event on main thread
            Task.Factory.StartNew (broadcast
                                  ,dispatch[0]
                                  ,Task.Factory.CancellationToken
                                  ,TaskCreationOptions.None
                                  ,scheduler);

          }
        }
      }
    }

    // helper method to trigger event upon receipt of server broadcast
    // NOTE: executed as short-term background task (see: proxyLoop)
    private void broadcast (Object state) { 
      // convert server broadcast into event instance
      var message = decode(state as Byte[]);
      var parts   = message.Split('\u221E');
      var args    = new MessageReceivedEventArgs(parts[0], parts[1]);
      // trigger event for listeners
      Debug.WriteLine("[{0}] {1}", DateTime.Now, message);
      if (MessageReceived != null) MessageReceived(this, args);
    }

    
    // helper method to convert message to bytes (for transmission to server)
    private Byte[] encode (String value) 
    { 
      return System.Text.Encoding.UTF8.GetBytes(value);
    }
    
    // helper method to convert bytes (received from server) to a string
    private String decode (Byte[] value) 
    { 
      return System.Text.Encoding.UTF8.GetString(value);
    }
    
    // helper method to construct a message for the server to re-broadcast
    private Byte[] message (String handle, String message)
    { 
      // message format: <handle>'\u221E'<message body>
      return encode(String.Format("{0}\u221E{1}", handle, message));
    }

    // helper method to ping server while the GUI is inactive
    private Byte[] keepAlive (String handle) 
    { 
      // ping format: <handle>
      return encode(handle); 
    }

  }
}
