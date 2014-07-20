using System;

namespace chatz.client
{
  public class SentMessage
  { 
    private readonly String   sentBy_;
    private readonly DateTime receivedAt_;
    private readonly String   content_;
    
    public SentMessage (String sentBy, String content)
    { 
      sentBy_      = sentBy;
      receivedAt_  = DateTime.UtcNow;
      content_     = content;
    }

    public String   SentBy      { get { return sentBy_;      } }
    public DateTime ReceivedAt  { get { return receivedAt_;  } }
    public String   Content     { get { return content_;     } }
  }

  public class MessageReceivedEventArgs :EventArgs
  {
    private readonly SentMessage message_;
    
    public MessageReceivedEventArgs(SentMessage message)
    { 
      message_ = message;
    }
    
    public MessageReceivedEventArgs(String sender, String message)
      : this (new SentMessage(sender, message)) { }

    public SentMessage Message { get { return message_; } }
  }

}
