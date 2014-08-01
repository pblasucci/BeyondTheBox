namespace dealz

open fszmq
open fszmq.Polling
open Microsoft.AspNet.SignalR
open Newtonsoft.Json
open System
open System.Globalization
open System.Threading
  
type date = System.DateTime
type time = System.TimeSpan

type agent<'T> = MailboxProcessor<'T>
type fetch<'R> = AsyncReplyChannel<'R>

type Order =
  { Stock   :string 
    Action  :BuySell
    Price   :float }
and BuySell = Buy | Sell

type ClientMsg =
  | Follow of stock:string
  | Forget of stock:string
  | Jabber of words:string
  | Reckon of trade:Order[]

type ServerMsg =
  | Acknowledged
  | Failed of error:string
  | Ticked of Tick
  | Memoed of Memo
  | Solved of Calc
and Tick =
  { Stock :string
    Stamp :date 
    Price :float }
and Memo =
  { Sender  :string
    Message :string }
and Calc =
  { Trade :Order[]
    Value :float }

type Client (follow,forget,jabber,reckon) as self =
  inherit PersistentConnection ()

  let decode message =
    try
      Some <| JsonConvert.DeserializeObject<ClientMsg> message
    with 
      | _ ->  //TODO: logging
              None

  let execute (action,input) connectionID =
    action input
    self.Connection.Send (string connectionID,Acknowledged)

  let fail error (connectionID :string) =
    self.Connection.Send (connectionID,Failed error)

  override self.OnReceived (_,connectionID,message) = 
    connectionID
    |>  match decode message with
        | Some (Follow stock) ->  execute (follow,stock)
        | Some (Forget stock) ->  execute (forget,stock)
        | Some (Jabber words) ->  execute (jabber,words)
        | Some (Reckon trade) ->  execute (reckon,trade)
        | None                ->  fail "Invalid Message"

module Services =
  
  type System.DateTime with
    static member UnixEpoch = date(1970,1,1,0,0,0,DateTimeKind.Utc)
    static member inline SinceEpoch offset =
      (offset :^a when ^a : (static member op_Explicit : ^a -> float))
      |> float
      |> date.UnixEpoch.AddSeconds 

  let encode = string >> System.Text.Encoding.UTF8.GetBytes
  let decode = System.Text.Encoding.UTF8.GetString
  
  let (|Tick|_|) raw =
    match Array.length raw with
    | 3 ->  let stock = decode raw.[0]
            let stamp = (raw.[1],0)
                        |> BitConverter.ToInt64 
                        |> date.SinceEpoch
            let price = (raw.[2],0)
                        |> BitConverter.ToDouble
            Some { Stock = stock; Stamp = stamp; Price = price }
    | _ ->  None
  
  let gotTick (proxy :IPersistentConnectionContext) =  
    Socket.recvAll
    >> (|Tick|_|)
    >> Option.map Ticked
    >> Option.iter (proxy.Connection.Broadcast >> ignore)

  let gotChat _ = ignore //TODO: ???

  let source = new CancellationTokenSource ()
  
  let agent  = new agent<ClientMsg> (fun inbox -> 
    async {
      let token = source.Token
      let proxy = GlobalHost.ConnectionManager.GetConnectionContext<Client>()

      use context   = new Context ()
      use chatz_req = Context.req context
      use chatz_sub = Context.sub context
      use tickz_sub = Context.sub context
      
      Socket.connect chatz_req "tcp://localhost:9001"
      Socket.connect chatz_sub "tcp://localhost:9002"
      Socket.connect tickz_sub "tcp://localhost:9003"

      let items = [ chatz_sub |> pollIn (gotChat proxy)
                    tickz_sub |> pollIn (gotTick proxy) ] 

      while not token.IsCancellationRequested do 
        items
        |> poll 500L // msec
        |> ignore

        if not token.IsCancellationRequested then
          let! msg = inbox.TryReceive 100
          msg |> Option.iter (function 
            | Follow stock -> Socket.subscribe   tickz_sub [encode stock]
            | Forget stock -> Socket.unsubscribe tickz_sub [encode stock]
            | _ -> ())
    })

  let start () = agent.Start ()
  let stop  () = 
    (source :> IDisposable).Dispose ()
    (agent  :> IDisposable).Dispose ()

  let follow stock = agent.Post (Follow stock)
  let forget stock = agent.Post (Forget stock)
