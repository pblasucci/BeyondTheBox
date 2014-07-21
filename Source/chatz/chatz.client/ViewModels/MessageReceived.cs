using System;

namespace chatz.client
{
  // Encapsulates the message data received from a server broadcast
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
    // Message originator
    public String SentBy { get { return sentBy_; } }
    // Timestamp when message was received (from server)
    public DateTime ReceivedAt { get { return receivedAt_; } }
    // Actual body of message
    public String Content { get { return content_; } }
  }

  // Represents the event of receiving a broadcast from the server
  public class MessageReceivedEventArgs :EventArgs
  {
    private readonly SentMessage message_;
    
    public MessageReceivedEventArgs(SentMessage message) { message_ = message; }
    
    public MessageReceivedEventArgs(String sender, String message)
      : this (new SentMessage(sender, message)) { }

    // An instance of the message data sent in the broadcast
    public SentMessage Message { get { return message_; } }
  }
}
