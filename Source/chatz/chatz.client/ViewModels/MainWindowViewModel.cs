using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace chatz.client
{
  public class MainWindowViewModel :INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    protected void Notify (String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private String handle_;
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
    
    private String input_;
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

    private readonly ObservableCollection<SentMessage> messages_;
    public ObservableCollection<SentMessage> Messages
    {
      get { return messages_; }
    }

    private readonly ChatzClient model;
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

    private readonly RelayCommand sendMessage;
    public RelayCommand SendMessage { get { return sendMessage; } }
  }
}
