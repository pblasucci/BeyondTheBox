Imports fszmq
Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.Text
Imports System.Threading

Public Class Main
  ' Facilitates thread-safe UI updates
  Delegate Sub NextTick(ByVal value As Double)
  
  Private Shared TokenSource  As New CancellationTokenSource()    ' Always cooperative cancellation between threads
  Private Shared StockQueue   As New ConcurrentQueue(Of String)() ' Used to pass current stock symbol to bg thread

  ' Implementation of NextTick delegate
  ' Updates UI with latest stock value
  Private Sub UpdateStockLabel(ByVal Value As Double)
    ' Change bg color based on stock trend (Green = Up, Red = Down)
    Dim Previous  = CDbl(StockLabel.Text) 
    Dim BgColor   = CType(IIf(Value > Previous, Color.Green, Color.Red), Color)
    
    StockLabel.BackColor  = BgColor
    StockLabel.Text       = String.Format("{0:F3}", Value)
  End Sub

  ' Update stock subscription, returning subscribed stock symbol
  Private Function CheckStock(Socket As Socket, OldStock As String) As String
    ' Check queue for new stock, disallow empty string
    Dim NewStock As String = ""
    If StockQueue.TryDequeue(NewStock) AndAlso Not String.IsNullOrWhiteSpace(NewStock) Then
      ' Remove previous subscription, if it exists
      If Not String.IsNullOrWhiteSpace(OldStock) Then
        Dim OldTopic = Encoding.UTF8.GetBytes(OldStock)
        Socket.Unsubscribe( { OldTopic } )
      End If
      ' Set actual subscription on socket
      Dim NewTopic = Encoding.UTF8.GetBytes(NewStock) 
      Socket.Subscribe( { NewTopic } )
          
      Return NewStock
    End If

    Return OldStock
  End Function

  ' Get next stock value from publisher
  Private Function GetNextTick(Socket As Socket, Stock As String) As Nullable(Of Double)
    ' Wait up to 1/2 second for value
    Dim Message As Byte()() = {} 
    If Socket.TryGetInput(500, Message) 
      ' simple message-structure check
      If Message.Length = 3 Then
        
        Dim Symbol  = Encoding.UTF8.GetString(Message(0))   ' Frame 1: Stock symbol (against which filtering occurs)
        Dim Value   = BitConverter.ToDouble(Message(2), 0)  ' Frame 3: Actual stock value
        'NOTE: Frame 2 (timestamp) of message is IGNORED!
          
        Debug.WriteLine("{0} = {1:F3}", Stock, Value)
        
        ' Validate subscription
        If Symbol <> Stock Then Throw New ApplicationException("Invalid Subscription!")

        Return New Nullable(Of Double)(Value)
      End If

      Debug.WriteLine("Malformed Message")
      Return New Nullable(Of Double)()
    End If
      
    Debug.WriteLine("Publisher Timed Out")
    Return New Nullable(Of Double)()
  End Function
  
  ' Workhorse routine, runs on bg thread until CancellationToken is signaled
  ' Manages Context, Socket
  ' Co-ordinates subscription changes and tick updates
  Private Sub TickLoop(ByVal Token As CancellationToken)
    Using Context = New Context() _
        , Socket  = Context.Sub()

      Socket.Connect("tcp://localhost:9003")
    
      Dim OldStock = ""
      While Not Token.IsCancellationRequested
        
        OldStock = CheckStock (Socket, OldStock) ' Update subscription, if necessary
        Dim Tick = GetNextTick(Socket, OldStock)  
        
        If Tick.HasValue Then  
          If Token.IsCancellationRequested Then Exit While
          ' Push new stock value to UI
          Dim State = CObj(Tick.Value)
          StockLabel.Invoke(SetNextTick, State)
        End If

      End While

    End Using
    
    Debug.WriteLine("Proxy Exited Cleanly")
  End Sub

  Private SetNextTick As New NextTick(AddressOf UpdateStockLabel)       ' Facilitates thread-safe UI updates
  Private TickProxy   As New Thread(Sub() TickLoop(TokenSource.Token))  ' Set up bg thread

  ' Initialization
  Private Sub Main_Load(Sender As Object, Args As EventArgs) Handles Me.Load
    TickProxy.Start() ' Start primary loop on bg thread
  End Sub
  
  ' Clean up
  Private Sub Main_FormClosing(Sender As Object, Args As FormClosingEventArgs) Handles Me.FormClosing
    TokenSource.Cancel()  ' Stop bg thread
    TickProxy.Join()      ' Wait for thread shutdown
  End Sub

  ' Respond to user's subscription change request
  Private Sub StockComboBox_SelectedIndexChanged(Sender As Object, Args As EventArgs) Handles StockComboBox.SelectedIndexChanged
    ' Reset UI
    StockLabel.BackColor  = Color.White
    ' Push new stock symbol to bg thread
    Dim Stock = CStr(StockComboBox.SelectedItem)
    StockQueue.Enqueue(Stock)
  End Sub

End Class
