module dealz.Host.Services

open fszmq
open fszmq.Polling
open System
open System.Diagnostics
   
/// sends messges to the client (via SignalR)
let dispatch message = Owin.resolve<IClientProxy>().Broadcast message

/// logs diagnotic data
let tracelog message = System.Diagnostics.Debug.WriteLine message

(* chatz service proxy *)

type ChatzMsg =
  | Jabber of report:string       // send message to group
  | Escape of signal:fetch<unit>  // shut down the proxy

let chatz context handle tell hear = agent<ChatzMsg>.Start (fun inbox -> 
  // create chatz sockets
  let chatz_req = Context.req context // send messgage to group (or send keep-alive)
  let chatz_sub = Context.sub context // receive group broadcast
  // configure sockets
  Socket.connect    chatz_req tell
  Socket.connect    chatz_sub hear
  Socket.subscribe  chatz_sub [ ""B ] // all topics

  // encapsulate sending a message (or keep-alive) and recving the result
  let exchange msg =
    msg
    |> encode
    |> Socket.send chatz_req
    // service always replys with a list of connected peers
    chatz_req
    |> Socket.recvAll
    |> Array.map decode
    |> Joined
    |> dispatch // pass list of peers to client

  // recursively check for input service then client
  let rec loop () = async {
    // check for input from service
    chatz_sub
    |> tryPollInput 500L<msec>
    |> Option.iter  (function 
                    | Memo m  ->  // got a message... pass it to client
                                  dispatch (Memoed m) 
                    | bytes   ->  bytes
                                  |> sprintf "Invalid Message: %A"
                                  |> tracelog) 
    // check for input from client
    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                ->  // no input... send "keep-alive"
                              exchange handle
                              return! loop ()
    | Some (Jabber report) -> // got message... send to service for broadcast
                              exchange (sprintf "%s\u221E%s" handle report)
                              return! loop ()
    | Some (Escape signal) -> // clean up sockets and shut down
                              dispatch Disconnect
                              [ chatz_req; chatz_sub; ] |> Seq.iter dispose
                              signal.Reply ()
                              return () }
  
  loop ())

(* tickz service proxy *)

type TickzMsg =
  | Follow of symbol:string       // start receiving ticks for symbol
  | Ignore of symbol:string       // stop receiving ticks for symbol
  | Cutoff of signal:fetch<unit>  // shut down the proxy

let tickz context address = agent<TickzMsg>.Start (fun inbox -> 
  // create tickz socket
  let tickz_sub = Context.sub context
  Socket.connect tickz_sub address  // recvs tick data for active subscriptions
    
  // recursively check for input from service then client
  let rec loop () = async {
    // check for input from service
    tickz_sub
    |> tryPollInput 500L<msec>
    |> Option.bind (function 
                    | Tick t  -> // got a Tick... pass it to client
                                 Some (Ticked t) 
                    | _       -> None) 
    |> Option.iter dispatch
    // check for input from client
    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                  ->  // no input... nothing to do
                                return! loop ()
    | Some (Follow symbol)  ->  // got input... add subscription
                                Socket.subscribe tickz_sub [encode symbol]
                                return! loop ()
    | Some (Ignore symbol)  ->  // got input... remove subscription
                                Socket.unsubscribe tickz_sub [encode symbol]
                                return! loop ()
    | Some (Cutoff signal)  ->  // clean up sockets and shut down
                                dispatch Disconnect
                                dispose tickz_sub
                                signal.Reply ()
                                return ()}
  
  loop ())

(* valuz service proxy *)

type ValuzMsg =
  | Reckon of trades:Order[]
  | Cancel of signal:fetch<unit>

let valuz context workerPath workerCount = agent<ValuzMsg>.Start (fun inbox -> 
  // create valuz sockets
  let valuz_push = Context.push context
  let valuz_pull = Context.pull context
  let valuz_pub  = Context.pub  context
  // configure valuz sockets
  Socket.bind valuz_push "tcp://*:9004" // sends trade order data to workers
  Socket.bind valuz_pull "tcp://*:9005" // gets ready signal and final value from workers
  Socket.bind valuz_pub  "tcp://*:9006" // sends kill signal to workers
   
  // recursively wait for desired number of workers to indicate readiness
  let rec waitForReady expected actual =
      if expected > actual then
        valuz_pull |> Socket.recv |> ignore
        waitForReady expected (actual + 1)
  
  // recursively wait for all workers to finish
  let rec waitForFinish expected values =
    if expected > List.length values
      then  let value = valuz_pull
                        |> Socket.recv
                        |> decode
                        |> float
            waitForFinish expected (value::values) // each final value (from worker) is added to all other final values
      else  values // all workers finished, return aggregate value
  
  // convert Order instance to message frames
  let framedOrder { Stock = stock; Action = action; Price = price; } = 
    [|  encode stock
        encode (match action with Buy -> +1 | _ -> -1)
        encode (string price)  |]
   
  // spawns a new worker process
  let launchWorker () =
    ProcessStartInfo(workerPath,WindowStyle=ProcessWindowStyle.Hidden)
    |> Process.Start
    |> ignore

  // recursively await a trade order
  let rec idle () = async {
    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                  ->  // no input... keep waiting
                                return! idle ()
    | Some (Reckon orders)  ->  // got input... switch to 'busy' mode
                                return! calculate orders
    | Some (Cancel signal)  ->  // clean up and shut down
                                dispatch Disconnect
                                [ valuz_push; valuz_pull; valuz_pub; ] |> Seq.iter dispose
                                signal.Reply ()
                                return () }
  // price out a trade order
  and calculate orders = async {
    // launch desired number of workers
    for _ in 1 .. workerCount do launchWorker ()
    // wait all workers to be ready
    waitForReady workerCount 0
    // convert each order to a frameset and dispatch to all workers
    orders 
    |> Seq.map framedOrder 
    |> Seq.iter (Socket.sendAll valuz_push)
    // aggregate results of each worker
    let values = waitForFinish (Array.length orders) []
    // shutdown workers
    Socket.send valuz_pub "batch.leave"B
    // send aggregate value to client
    dispatch  (Solved { Value = List.sum values })
    // switch back to 'idle' mode
    return! idle () }
  
  idle ())

//NOTE: shutting down any service proxy, shuts down the client connection!
let start handle (chatzTell,chatzHear,tickzLink) workerPath workerCount =
  // create one context to use with all service sockets
  let context = new Context ()
  // launch service proxies (ie: MailboxProcessors)
  let chatz = chatz context handle chatzTell chatzHear
  let tickz = tickz context tickzLink
  let valuz = valuz context workerPath workerCount
 
  let binnable : IDisposable list = [context; chatz; tickz; valuz]

  // create facade for services
  { new IServiceProxy with
      member __.Jabber report = chatz <-- Jabber report // send message to group
      member __.Follow symbol = tickz <-- Follow symbol // start recving stock data
      member __.Forget symbol = tickz <-- Ignore symbol // stop recving stock data
      member __.Reckon trades = valuz <-- Reckon trades // price trade order

    interface IDisposable with
      member __.Dispose () =
        // shut down each service proxy
        chatz --> Escape
        tickz --> Cutoff
        valuz --> Cancel
        // clean up resources
        binnable |> List.iter dispose }
