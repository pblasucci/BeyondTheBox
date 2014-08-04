namespace dealz

type date = System.DateTime
type time = System.TimeSpan

type agent<'T> = MailboxProcessor<'T>
type fetch<'R> = AsyncReplyChannel<'R>

type [<Measure>] msec


[<RequireQualifiedAccess>]
module String =

  open System
  
  let split (seperator :string) (value :string) =
    value.Split (seperator.ToCharArray(),StringSplitOptions.RemoveEmptyEntries)

[<RequireQualifiedAccess>]
module Owin =
  
  open Owin
  open Microsoft.Owin.Hosting
  open Microsoft.Owin.StaticFiles
  open Microsoft.AspNet.SignalR
  open System

  let useStaticFiles (app :IAppBuilder) = app.UseStaticFiles ()
  
  let mapSignalR<'T when 'T :> PersistentConnection> path (app :IAppBuilder) = app.MapSignalR<'T> path

  let start url config = WebApp.Start (string url,Action<IAppBuilder>(config))

  let addResolver<'T> resolve = GlobalHost.DependencyResolver.Register (typeof<'T>,(fun () -> box<'T> <| resolve())) 

  let resolve<'T> () = GlobalHost.DependencyResolver.Resolve<'T> () 


type Memo =
  { Sender  :string
    Message :string }

type Tick =
  { Stock :string
    Stamp :date 
    Price :float }

type Order =
  { Stock   :string 
    Action  :BuySell
    Price   :float }
and BuySell = Buy | Sell

type Calc =
  { Value :float }

type ClientMsg =
  | Follow of symbol:string
  | Forget of symbol:string
  | Jabber of report:string
  | Reckon of trades:Order[]

type ServerMsg =
  | Acknowledged
  | Failed of error:string
  | Ticked of Tick
  | Memoed of Memo
  | Joined of users:string[]
  | Solved of Calc
  | Disconnect

type IServiceProxy =
  inherit System.IDisposable

  abstract Follow : stock:string  -> unit
  abstract Forget : stock:string  -> unit
  abstract Jabber : words:string  -> unit
  abstract Reckon : trade:Order[] -> unit

type IClientProxy =
  abstract Broadcast : ServerMsg -> unit


[<AutoOpen>]
module Library = 

  open Newtonsoft.Json
  open System

  let inline dispose (o :^o :> IDisposable) = o.Dispose ()
  
  let inline (|Success|Failure|) value = 
    match value with
    | Choice1Of2 value -> Success value
    | Choice2Of2 value -> Failure value

  let toJSON      value = JsonConvert.SerializeObject value
  let ofJSON<'T>  value = JsonConvert.DeserializeObject<'T> value 
  let tryJSON<'T> value = try
                            value |> ofJSON<ClientMsg> |> Choice1Of2
                          with 
                            | x ->  Choice2Of2 x.Message

  let encode = string >> System.Text.Encoding.UTF8.GetBytes
  let decode = System.Text.Encoding.UTF8.GetString

  type System.DateTime with
    static member UnixEpoch = date(1970,1,1,0,0,0,DateTimeKind.Utc)
    static member inline SinceEpoch offset =
      (offset :^a when ^a : (static member op_Explicit : ^a -> float))
      |> float
      |> date.UnixEpoch.AddSeconds 
      
  type Microsoft.FSharp.Control.MailboxProcessor<'T> with
    member self.TryReceive (timeout :int<'u>) = self.TryReceive (int timeout)

  let (|Str|_|) raw =
    try
      Some (decode raw)
    with
      | _ -> None
    
  let (|Dbl|_|) raw =
    try
      (raw,0)
      |> BitConverter.ToDouble
      |> Some
    with
      | _ -> None

  let (|Dts|_|) raw =
    try
      (raw,0)
      |> BitConverter.ToInt64
      |> date.SinceEpoch
      |> Some
    with
      | _ -> None

  let (|Memo|_|) msg =
    match Array.length msg with
    | 1 ->  match msg.[0] with
            | Str frame ->  match frame |> String.split "\u221E" with
                            | [| sender; memo; |] -> Some { Sender  = sender
                                                            Message = memo }
                            | _                   -> None
            | _                                   -> None
    | _                                           -> None

  let (|Tick|_|) msg =
    match Array.length msg with
    | 3 ->  match msg.[0],msg.[1],msg.[2] with
            | Str stock,Dts stamp,Dbl price -> Some { Stock = stock
                                                      Stamp = stamp
                                                      Price = price }
            | _                             -> None
    | _                                     -> None

  let (|Calc|_|) msg =
    match Array.length msg with
    | 1 ->  match msg.[0] with
            | Dbl value -> Some { Value = value }
            | _         -> None
    | _                 -> None
