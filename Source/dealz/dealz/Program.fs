namespace dealz

module Server =
  
  open fszmq
  open Owin
  open Microsoft.Owin.Hosting
  open Microsoft.Owin.StaticFiles
  open Microsoft.AspNet.SignalR
  open System
  open System.Threading.Tasks

  type SignalRProxy() =
    inherit PersistentConnection()

    let context = new Context()
    let socket  = Context.sub context
    do  Socket.connect socket "tcp://localhost:9003"
        Socket.subscribe socket ["MSFT"B]
        Async.Start(async {
          while true do
            let msg   = Socket.recvAll socket
            let value = sprintf "MSFT %f" (BitConverter.ToDouble (msg.[2],0))
            Diagnostics.Debug.WriteLine (value)
            let c = GlobalHost.ConnectionManager.GetConnectionContext<SignalRProxy>()
            c.Connection.Broadcast value |> ignore
        })

    override self.OnReceived (request,connectionID,message) = 
      //TODO: ???
      self.Connection.Broadcast(message)

  let start () = 
    WebApp.Start ("http://localhost:9000",fun app -> 
      let app = app.UseStaticFiles ()
      let app = app.MapSignalR<SignalRProxy> "/signalr"
      ignore app)
    
module Program =

  open System.Diagnostics
  
  let [<Literal>] OKAY = 0

  let iscanfn = System.Console.ReadLine >> ignore

  [<EntryPoint>]
  let Main _ =
    use _app = Server.start()
    printfn "Press <RETURN> to exit"; iscanfn ()
    OKAY
