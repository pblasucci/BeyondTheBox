module dealz.Services

open fszmq
open fszmq.Polling
open System
   
let dispatch message = Owin.resolve<IClientProxy>().Broadcast message


type ChatzMsg =
  | Jabber  of words:string
  | Abandon of fetch<unit>

let chatz context handle =
  agent<ChatzMsg>.Start (fun inbox -> async {
  
    use chatz_req = Context.req context
    use chatz_sub = Context.sub context
    Socket.connect    chatz_req "tcp://localhost:9001"
    Socket.connect    chatz_sub "tcp://localhost:9002"
    Socket.subscribe  chatz_sub [ ""B ] // all topics
    
    let tellServer msg =
      msg
      |> encode
      |> Socket.send chatz_req
    
    let updateGroup () = 
      chatz_req
      |> Socket.recvAll
      |> Array.map decode
      |> Joined
      |> dispatch

    let loop = ref true
    while !loop do 
      // get data from service
      chatz_sub
      |> tryPollInput 500L<msec>
      |> Option.bind (function 
                      | Memo m  -> Some (Memoed m) 
                      | _       -> None) 
      |> Option.iter dispatch

      // get updates from client
      let!  msg = inbox.TryReceive 100 // msec
      match msg with
      | None                ->  // send keep-alive
                                tellServer handle
                                updateGroup ()
      | Some (Jabber words) ->  // send message
                                tellServer <| sprintf "%s\u221E%s" handle words
                                updateGroup ()
      | Some (Abandon chan) ->  // shutdown
                                loop := false 
                                dispatch Disconnect
                                chan.Reply ()
  })


type TickzMsg =
  | Follow  of stock:string
  | Forget  of stock:string
  | Abandon of fetch<unit>

let tickz context = 
  agent<TickzMsg>.Start (fun inbox -> async {

    use tickz_sub = Context.sub context
    Socket.connect tickz_sub "tcp://localhost:9003"
    
    let loop = ref true
    while !loop do 
      // get data from service
      tickz_sub
      |> tryPollInput 500L<msec>
      |> Option.bind (function 
                      | Tick t  -> Some (Ticked t) 
                      | _       -> None) 
      |> Option.iter dispatch

      // get updates from client
      let! msg = inbox.TryReceive 100 // msec
      msg 
      |> Option.iter (function 
          // change topics
          | Follow stock -> Socket.subscribe   tickz_sub [encode stock]
          | Forget stock -> Socket.unsubscribe tickz_sub [encode stock]
          // shutdown
          | Abandon chan -> loop := false 
                            dispatch Disconnect
                            chan.Reply ())
  })


let start handle =
  let context = new Context ()
  let chatz   = chatz context handle
  let tickz   = tickz context
  
  { new IServiceProxy with
      member __.Jabber words = chatz.Post (Jabber words)
      member __.Follow stock = tickz.Post (Follow stock)
      member __.Forget stock = tickz.Post (Forget stock)
      member __.Reckon trade = ()//valuz <-- Reckon trade

    interface IDisposable with
      member __.Dispose () =
        chatz.PostAndReply ChatzMsg.Abandon
        tickz.PostAndReply TickzMsg.Abandon
        dispose chatz
        dispose tickz
        dispose context }
