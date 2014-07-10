Imports fszmq
Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.Text
Imports System.Threading

Public Class Main

  Private Shared TopicQueue   As New ConcurrentQueue(Of String)()
  Private Shared TokenSource  As New CancellationTokenSource()

  Delegate Sub UpdateValue(ByVal value As Double)
  Private DoUpdateValue As UpdateValue = New UpdateValue(AddressOf UpdateValueLabel)
  Private Sub UpdateValueLabel(ByVal value As Double)
    Dim Previous = CDbl(StockLabel.Text)
    StockLabel.BackColor = CType(IIf(value > Previous, Color.Green, Color.Red), Color)
    StockLabel.Text = String.Format("{0:F3}", value)
    Previous = value
  End Sub

  Private Sub PollTopic(ByVal o As Object)
    Dim Token     = CType(o, CancellationToken)
    Dim Previous  = ""
    Using Context = New Context() _
        , Socket  = Context.Sub()
      Socket.Connect("tcp://localhost:9003")
      While Not Token.IsCancellationRequested
        Dim Topic As String = ""
        If TopicQueue.TryDequeue(Topic) Then
          If Not String.IsNullOrWhiteSpace(Previous) Then
            Socket.Unsubscribe( { Encoding.UTF8.GetBytes(Previous) } )
          End If
          Socket.Subscribe  ( { Encoding.UTF8.GetBytes(Topic   ) } )
          Previous = Topic
        End If
        Dim Message As Byte()() = {} 
        If Socket.TryGetInput(1000, Message) AndAlso Message.Length = 3 Then
          Dim Symbol  = Encoding.UTF8.GetString(Message(0))
          Dim Value   = BitConverter.ToDouble(Message(2), 0)
          Debug.WriteLine("{0} = {1:F3}", Symbol, Value)
          If Not Token.IsCancellationRequested Then
            StockLabel.Invoke(DoUpdateValue,CObj(value))
          End If
        End If
      End While
    End Using
    Debug.WriteLine("Proxy Exited Cleanly")
  End Sub

  Private Proxy As New Thread(AddressOf PollTopic)

  Private Sub Main_Load(ByVal sender As Object, ByVal e As EventArgs) _ 
    Handles Me.Load
    Proxy.Start(TokenSource.Token)
  End Sub

  Private Sub Main_FormClosing(ByVal sender As Object, Byval e As FormClosingEventArgs) _ 
    Handles Me.FormClosing
    TokenSource.Cancel()
    Proxy.Join()
  End Sub


  Private Sub StockComboBox_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) _ 
    Handles StockComboBox.SelectedIndexChanged
    StockLabel.BackColor = Color.White
    TopicQueue.Enqueue(CStr(StockComboBox.SelectedItem))
  End Sub

End Class
