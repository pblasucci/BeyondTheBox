﻿namespace dealz

open Microsoft.AspNet.SignalR
open System.Diagnostics

type Client (proxy :IServiceProxy) =
  inherit PersistentConnection ()

  override self.OnReceived (_,connectionID,message) = 
    let response =  match tryJSON message with
                    | Success (Follow symbol) ->  proxy.Follow symbol
                                                  Acknowledged
                    | Success (Forget symbol) ->  proxy.Forget symbol
                                                  Acknowledged
                    | Success (Jabber report) ->  proxy.Jabber report
                                                  Acknowledged
                    | Success (Reckon trades) ->  proxy.Reckon trades
                                                  Acknowledged
                    | Failure (failureReport) ->  Debug.WriteLine failureReport
                                                  Failed failureReport
      
    self.Connection.Send (connectionID,response)


module Program =

  let [<Literal>] OKAY = 0

  let iscanfn = System.Console.ReadLine >> ignore

  let clientProxy () =
    {new IClientProxy with
      member __.Broadcast message = 
        let proxy = GlobalHost.ConnectionManager
                              .GetConnectionContext<Client> ()
        message 
        |> proxy.Connection.Broadcast 
        |> ignore}

  let configure svc app =
    (fun () -> Client svc   ) |> Owin.addResolver       
    (fun () -> clientProxy()) |> Owin.addResolver
        
    app 
    |> Owin.useStaticFiles
    |> Owin.mapSignalR<Client> "/signalr"
    |> ignore

  [<EntryPoint>]
  let Main args =
    let svc = Services.start (args.[0]) //TODO: handle this better
    let app = Owin.start "http://localhost:9000" (configure svc)
  
    printfn "Press <RETURN> to exit"; iscanfn ()
    svc.Dispose()
    app.Dispose()
    OKAY