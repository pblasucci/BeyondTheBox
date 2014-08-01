module dealz.Program 

open Owin
open Microsoft.Owin.Hosting
open Microsoft.Owin.StaticFiles
open Microsoft.AspNet.SignalR
    
let [<Literal>] OKAY = 0

let iscanfn = System.Console.ReadLine >> ignore

[<EntryPoint>]
let Main _ =
  use _app = WebApp.Start ("http://localhost:9000",fun app -> 
    GlobalHost.DependencyResolver.Register (typeof<Client>
                                            ,fun () -> box <| Client(Services.follow
                                                                    ,Services.forget
                                                                    ,ignore
                                                                    ,ignore))
    let app = app.UseStaticFiles ()
    let app = app.MapSignalR<Client> "/signalr"
    ignore app)
  Services.start ()
  printfn "Press <RETURN> to exit"; iscanfn ()
  Services.stop ()
  OKAY
