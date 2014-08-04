module dealz.Services

open fszmq
open fszmq.Polling
open System
open System.Diagnostics
   
let dispatch message = Owin.resolve<IClientProxy>().Broadcast message
let tracelog message = System.Diagnostics.Debug.WriteLine message


type ChatzMsg =
  | Jabber of report:string
  | Escape of signal:fetch<unit>

let chatz context handle = agent<ChatzMsg>.Start (fun inbox -> 
  let chatz_req = Context.req context
  let chatz_sub = Context.sub context
  
  Socket.connect    chatz_req "tcp://localhost:9001"
  Socket.connect    chatz_sub "tcp://localhost:9002"
  Socket.subscribe  chatz_sub [ ""B ] // all topics
    
  let exchange msg =
    msg
    |> encode
    |> Socket.send chatz_req
     
    chatz_req
    |> Socket.recvAll
    |> Array.map decode
    |> Joined
    |> dispatch

  let rec loop () = async {
    chatz_sub
    |> tryPollInput 500L<msec>
    |> Option.iter  (function 
                    | Memo m  ->  dispatch (Memoed m) 
                    | bytes   ->  bytes
                                  |> sprintf "Invalid Message: %A"
                                  |> tracelog) 
    

    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                ->  exchange handle
                              return! loop ()
    | Some (Jabber report) -> exchange (sprintf "%s\u221E%s" handle report)
                              return! loop ()
    | Some (Escape signal) -> dispatch Disconnect
                              [ chatz_req; chatz_sub; ] |> Seq.iter dispose
                              signal.Reply ()
                              return () }
  
  loop ())


type TickzMsg =
  | Follow of symbol:string
  | Ignore of symbol:string
  | Cutoff of signal:fetch<unit>

let tickz context = agent<TickzMsg>.Start (fun inbox -> 

  let tickz_sub = Context.sub context
  Socket.connect tickz_sub "tcp://localhost:9003"
    
  let rec loop () = async {
    tickz_sub
    |> tryPollInput 500L<msec>
    |> Option.bind (function 
                    | Tick t  -> Some (Ticked t) 
                    | _       -> None) 
    |> Option.iter dispatch

    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                  ->  return! loop ()
    | Some (Follow symbol)  ->  Socket.subscribe tickz_sub [encode symbol]
                                return! loop ()
    | Some (Ignore symbol)  ->  Socket.unsubscribe tickz_sub [encode symbol]
                                return! loop ()
    | Some (Cutoff signal)  ->  dispatch Disconnect
                                dispose tickz_sub
                                signal.Reply ()
                                return ()}
  
  loop ())


type ValuzMsg =
  | Reckon of trades:Order[]
  | Cancel of signal:fetch<unit>

let valuz context workerPath workerCount = agent<ValuzMsg>.Start (fun inbox -> 
  
  let valuz_push = Context.push context
  let valuz_pull = Context.pull context
  let valuz_pub  = Context.pub  context

  Socket.bind valuz_push "tcp://*:9004"
  Socket.bind valuz_pull "tcp://*:9005"
  Socket.bind valuz_pub  "tcp://*:9006"
   
  let rec waitForReady expected actual =
      if expected > actual then
        valuz_pull |> Socket.recv |> ignore
        waitForReady expected (actual + 1)
    
  let rec waitForFinish expected values =
    if expected > List.length values
      then  let value = valuz_pull
                        |> Socket.recv
                        |> decode
                        |> float
            waitForFinish expected (value::values)
      else  values
    
  let framedOrder { Stock = stock; Action = action; Price = price; } = 
    [|  encode stock
        encode (match action with Buy -> +1 | _ -> -1)
        encode (string price)  |]
   
  let launchWorker () =
    ProcessStartInfo(workerPath,WindowStyle=ProcessWindowStyle.Hidden)
    |> Process.Start
    |> ignore

  let rec idle () = async {
    let!  msg = inbox.TryReceive 100<msec>
    match msg with
    | None                  ->  return! idle ()
    | Some (Reckon orders)  ->  return! calculate orders
    | Some (Cancel signal)  ->  
        dispatch Disconnect
        [ valuz_push; valuz_pull; valuz_pub; ] |> Seq.iter dispose
        signal.Reply ()
        return () }

  and calculate orders = async {
    for _ in 1 .. workerCount do launchWorker ()
    waitForReady  workerCount 0

    orders 
    |> Seq.map framedOrder 
    |> Seq.iter (Socket.sendAll valuz_push)

    let values = waitForFinish (Array.length orders) []
    Socket.send valuz_pub "batch.leave"B
    dispatch  (Solved { Value = List.sum values })
    return! idle () }
  
  idle ())


let start handle workerPath workerCount =
  let context = new Context ()

  let chatz = chatz context handle
  let tickz = tickz context
  let valuz = valuz context workerPath workerCount
  
  { new IServiceProxy with
      member __.Jabber report = chatz.Post (Jabber report)
      member __.Follow symbol = tickz.Post (Follow symbol)
      member __.Forget symbol = tickz.Post (Ignore symbol)
      member __.Reckon trades = valuz.Post (Reckon trades)

    interface IDisposable with
      member __.Dispose () =
        chatz.PostAndReply Escape
        tickz.PostAndReply Cutoff
        valuz.PostAndReply Cancel
        dispose chatz
        dispose tickz
        dispose valuz
        dispose context }
