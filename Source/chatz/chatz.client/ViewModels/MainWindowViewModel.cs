using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using messageList = System.Collections.ObjectModel.ObservableCollection<chatz.client.SentMessage>;

namespace chatz.client
{
  public class MainWindowViewModel :INotifyPropertyChanged
  {
    private readonly  ChatzClient   model;
    private readonly  RelayCommand  sendMessage;
    private readonly  messageList   messages_;
    private           String        handle_;
    private           String        input_;
    
    public MainWindowViewModel (ChatzClient model)
    { 
      messages_ = new ObservableCollection<SentMessage>();
      messages_.CollectionChanged += (_, e) => { Notify("Messages"); };
      
      this.model = model;
      model.MessageReceived += (_, e) => { messages_.Add(e.Message); };
      
      Handle = model.Handle;

      sendMessage = new RelayCommand(_ => { this.model.Send(this.Input ?? "");
                                            this.Input = null; });
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public RelayCommand SendMessage { get { return sendMessage; } }
    
    public messageList Messages { get { return messages_; } }

    public  String Handle
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
    
    public  String Input
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

    protected void Notify (String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }    
  }
}
