Imports fszmq
Imports System.Runtime.CompilerServices
Imports System.Text

Module Program

  Const ADDRESS As String   = "tcp://localhost:9003"
  Const TIMEOUT As Integer  = 1000 ' milliseconds

  ' used to co-ordinate interrupts with program shutdown
  Dim ShouldExit As Boolean = False

  ' enables clean shut down in response to CTRL+C
  Private Sub Cancel(sender As Object, e As ConsoleCancelEventArgs)
    e.Cancel    = True ' tell system we've got this
    ShouldExit  = True ' tell program to shutdown
  End Sub

  ' converts C timestampe in CLR Dates
  Private Function ToDate(stamp As Long) As Date
    ' convert stamp to TimeSpan
    Dim ts = TimeSpan.FromTicks(stamp * TimeSpan.TicksPerSecond)
    ' add stamp (as TimeSpan) to Unix epoch
    Dim dt = New Date(1970, 1, 1).Add(ts)
    ' translate to local timezone
    Return TimeZone.CurrentTimeZone.ToLocalTime(dt)
  End Function

  ' write output to a specific row (line) in the console buffer
  Private Sub WriteLine( index   As Integer _
                      , format  As String  _
                      , ParamArray args As Object() )
    ' move to desired line
    Console.SetCursorPosition(0,index)
    ' blank out line
    Console.WriteLine()
    ' stay on desired line
    Console.SetCursorPosition(0,index)
    ' write actual message
    Console.WriteLine(format, args)
  End Sub

  ' write diagnostic messages starting a certain offset in the output buffer
  Private Sub LogLine(message As String)
    ' HACK: keep track of last line in "log" section of output buffer
    Dim Static line As Integer = 5 
    ' get timestamp
    Dim stamp = TimeZone.CurrentTimeZone.ToLocalTime(Date.UtcNow)
    ' write stamped message to "log" section of output buffer
    WriteLine(line, "[{0}] {1}", stamp, message)
    ' update next log line
    line += 1
  End Sub

  ' displays HELP message when program is given invalid input
  Private Sub ShowUsage()
    Console.Clear()
    Console.WriteLine("usage: tickz.client <stock-symbol>")
  End Sub
  
  ' writes greeting and initial log messages
  Private Sub SetUp(topic As String)
    Console.Clear()
    Console.WriteLine("!!! WELCOME TO tickz !!!")
    Console.WriteLine()

    WriteLine(4, ":: INFO ::", Nothing)
    LogLine(String.Format("Connected to:  {0}", ADDRESS))
    LogLine(String.Format("Subscriptions: {0}", topic  ))
  End Sub

  ' updates subscribed stock data, after calculating price trend (up or down)
  Private Sub UpdateValue(topic As String, value As Double, stamp As Long)
    ' HACK: record last stock value for trend calculation
    Dim Static previous As Double = 0.0
    ' determine direction of stock price movement
    Dim trend = IIf(value > previous, "+", "-")
    ' update display
    WriteLine(2, "[{0}] {1} = {2:C3} {3}", ToDate(stamp), topic, value, trend)
    ' update history (for next iteration)
    previous = value
  End Sub

  Private Sub TearDown()
    Console.Clear()
    Console.WriteLine("Good Bye.")
  End Sub

  ' SUB tcp://localhost:9003
  ' >>  [ "(?stock:[A-Z][A-Z0-9]+)" ]
  '     [ timestamp :f64            ]
  '     [ price     :f64            ]
  Public Sub Main()
    ' set up interrupt handling
    AddHandler Console.CancelKeyPress, AddressOf Cancel

    ' get inputs
    Dim args = My.Application.CommandLineArgs
    If  args.Count <> 1 OrElse String.IsNullOrWhiteSpace(args(0)) then
      ShowUsage()
      Environment.Exit(-1)
    End If

    Dim topic = args(0) 'TODO: add better validation

    ' initialize subscriber socket
    Using Context = New Context() _
        , Subscriber = Context.Sub()

      With Subscriber
        .SetOption(ZMQ.RCVTIMEO, TIMEOUT)
        .Subscribe({ Encoding.UTF8.GetBytes(topic) })
        .Connect(ADDRESS)
      End With

      ' initialize output
      SetUp(topic)
      
      ' loop until interrupted
      While Not ShouldExit
      
        Try
          Dim message = Subscriber.RecvAll()
          Dim symbol  = Encoding.UTF8.GetString(message(0)) ' Frame 1: stock symbol
      
          Select symbol = topic
          
            Case True
              Dim stamp = BitConverter.ToInt64 (message(1), 0) ' Frame 2: timestamp
              Dim value = BitConverter.ToDouble(message(2), 0) ' Frame 3: new value
              
              UpdateValue(topic, value, stamp)

            Case False ' NOTE: this should never happen
              LogLine(String.Format("Wrong Symbol '{0}'", symbol))
          
          End Select
      
        Catch ex As TimeoutException
          LogLine(String.Format("Recv Timeout ({0} ms)", TIMEOUT))
          
        End Try
      
      End While

      TearDown()

    End Using
  
  End Sub

End Module
