namespace dealz.Host

open Microsoft.AspNet.SignalR
open System.Diagnostics

/// server-side SignalR endpoint
type Client (proxy :IServiceProxy) =
  inherit PersistentConnection ()

  override self.OnReceived (_,connectionID,message) =
    // decode input message
    // if valid, send to service proxy
    // construct response
    let response =  match tryJSON message with
                    | Success (Follow symbol) ->  // group chat message
                                                  proxy.Follow symbol
                                                  Acknowledged
                    | Success (Forget symbol) ->  // start getting ticks for stock
                                                  proxy.Forget symbol
                                                  Acknowledged
                    | Success (Jabber report) ->  // stop getting ticks for stock
                                                  proxy.Jabber report
                                                  Acknowledged
                    | Success (Reckon trades) ->  // price a trade order
                                                  proxy.Reckon trades
                                                  Acknowledged
                    | Failure (failureReport) ->  // unknown message from client
                                                  Debug.WriteLine failureReport
                                                  Failed failureReport
    // send response back to client
    self.Connection.Send (connectionID,response)


module Program =

  let [<Literal>] OKAY = 0

  let iscanfn = System.Console.ReadLine >> ignore

  /// creates a new instance of the client facade services use to interact with SignalR
  let clientProxy () =
    {new IClientProxy with
      member __.Broadcast message = 
        let proxy = GlobalHost.ConnectionManager
                              .GetConnectionContext<Client> ()
        message 
        |> proxy.Connection.Broadcast 
        |> ignore}

  /// sets up OWIN web server
  let configure svc app =
    // register client and services facade with IoC container
    (fun () -> Client svc   ) |> Owin.addResolver       
    (fun () -> clientProxy()) |> Owin.addResolver
    // configer web server features
    app 
    |> Owin.useStaticFiles
    |> Owin.mapSignalR<Client> "/signalr"
    |> ignore

  [<EntryPoint>]
  let Main args =
    // get command-line arguments
    let handle,chatz_tell,chatz_hear,tickz,workerPath,workerCount = 
      match args with
      | [|  handle; 
            remote; 
            path; 
            count |] ->   handle
                        , sprintf "tcp://%s:9001" remote
                        , sprintf "tcp://%s:9002" remote
                        , sprintf "tcp://%s:9003" remote
                        , path
                        , int count
      | _            -> invalidOp "dealz not properly configured"
    // launch service proxes
    let svc = Services.start handle (chatz_tell,chatz_hear,tickz) workerPath workerCount
    // launch web server
    let app = Owin.start "http://*:9000" (configure svc)
    // wait for termination
    printf "Press <return> to exit "; iscanfn ()
    // clean up
    svc.Dispose()
    app.Dispose()
    OKAY
