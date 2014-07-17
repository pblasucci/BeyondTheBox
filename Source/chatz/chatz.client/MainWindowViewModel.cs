using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace chatz.client
{
  public class MainWindowViewModel :INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private String handle_;
    public  String Handle
    {
      get { return handle_; }
      set
      { 
        handle_ = value;
        Notify("Handle");
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
    }

    protected void Notify (String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }
}
