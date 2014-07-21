using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using messageList = System.Collections.ObjectModel.ObservableCollection<chatz.client.SentMessage>;

namespace chatz.client
{
  // Coordinates between the server proxy and the GUI
  public class MainWindowViewModel :INotifyPropertyChanged
  {
    // Proxy connection to chat server
    private readonly  ChatzClient   model;        
    private readonly  RelayCommand  sendMessage;
    private readonly  messageList   messages_;
    private           String        handle_;
    private           String        input_;
    
    public MainWindowViewModel (ChatzClient model)
    { 
      // initialize list of received broadcasts
      messages_ = new ObservableCollection<SentMessage>();
      // receiving a broadcast triggers INotifyPropertyChanged
      messages_.CollectionChanged += (_, e) => { Notify("Messages"); };
      
      // store proxy connection to chat server
      this.model = model;
      // handle receipt of broadcst message
      model.MessageReceived += (_, e) => { messages_.Add(e.Message); };
      
      // expose handle to GUI
      Handle = model.Handle;

      // wire-up command to pass messages to server; resets GUI input
      sendMessage = new RelayCommand(_ => { this.model.Send(this.Input ?? "");
                                            this.Input = null; });
    }

    // INotifyPropertyChanged implementation
    // Required for WPF data binding
    public event PropertyChangedEventHandler PropertyChanged;

    // GUI invokes this command to pass messages to the server
    public RelayCommand SendMessage { get { return sendMessage; } }
    
    // Running list of messages received from a server broadcast; bound to GUI
    public messageList Messages { get { return messages_; } }

    // Handle with which client connects to server; bound to GUI
    public String Handle
    {
      get { return handle_; }
      set
      { 
        if (value != handle_)
        {
          handle_ = value;
          Notify("Handle");
        }
      }
    }
    
    // Input to be sent to the server; biund to GUI
    public String Input
    {
      get { return input_; }
      set
      { 
        if (value != input_)
        {
          input_ = value;
          Notify("Input");
        }
      }
    }

    // Helper method for implementing INotifyPropertyChanged
    protected void Notify (String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }    
  }
}
