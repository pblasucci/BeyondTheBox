namespace dealz.Host

(* aliases and units-of-measure to make code read better *)

type date = System.DateTime
type time = System.TimeSpan

type agent<'T> = MailboxProcessor<'T>
type fetch<'R> = AsyncReplyChannel<'R>

type [<Measure>] msec

/// helper functions for string manipulation
[<RequireQualifiedAccess>]
module String =

  open System

  /// splits string based on presence of a seperator
  let split (seperator :string) (value :string) =
    value.Split (seperator.ToCharArray(),StringSplitOptions.RemoveEmptyEntries)

/// helper functions to make OWIN more "F# friendly"
[<RequireQualifiedAccess>]
module Owin =
  
  open Owin
  open Microsoft.Owin.Hosting
  open Microsoft.Owin.StaticFiles
  open Microsoft.AspNet.SignalR
  open System

  /// enables hosting of static files from disk
  let useStaticFiles (app :IAppBuilder) = app.UseStaticFiles ()

  /// exposes SignalR at the given path
  let mapSignalR<'T when 'T :> PersistentConnection> path (app :IAppBuilder) = app.MapSignalR<'T> path

  /// kick-starts the OWIN web hosting process
  let start url config = WebApp.Start (string url,Action<IAppBuilder>(config))

  /// add a custom dependency-injection resolver for the given type
  let addResolver<'T> resolve = GlobalHost.DependencyResolver.Register (typeof<'T>,(fun () -> box<'T> <| resolve())) 

  /// gets an instance of the given type from the inversion-of-control container
  let resolve<'T> () = GlobalHost.DependencyResolver.Resolve<'T> () 


(* Domain Types *)

/// represents a message in the chatz service
type Memo =
  { Sender  :string
    Message :string }

/// represents a discrete point-in-time stock price in the tickz service
type Tick =
  { Stock :string
    Stamp :date 
    Price :float }

/// represents an trade order in the valuz service
type Order =
  { Stock   :string 
    Action  :BuySell
    Price   :float }
and BuySell = Buy | Sell

/// represents the result of pricing a trade order via the valuz service
type Calc =
  { Value :float }

/// defines the messages sent, via SignalR, from the client to the web server
type ClientMsg =
  | Follow of symbol:string
  | Forget of symbol:string
  | Jabber of report:string
  | Reckon of trades:Order[]

/// defines the messages sent, via SignalR, from the web server to the client
type ServerMsg =
  | Acknowledged              // sent from web server
  | Failed of error:string    // sent from web server
  | Ticked of Tick            // sent from tickz service
  | Memoed of Memo            // sent from chatz service
  | Joined of users:string[]  // sent from chatz service
  | Solved of Calc            // sent from valuz service
  | Disconnect                // sent from web server

/// provides an API for the tickz, chatz, and valuz services (which is consumed in the web server)
type IServiceProxy =
  inherit System.IDisposable
  /// start tracking tick data for a stock
  abstract Follow : stock:string  -> unit
  /// stop tracking tick data fro a stock
  abstract Forget : stock:string  -> unit
  /// send a chat message to the group
  abstract Jabber : words:string  -> unit
  /// price a trade order
  abstract Reckon : trade:Order[] -> unit

/// provides a DI-friendly way for services to send messages back to the client (via SignalR)
type IClientProxy =
  abstract Broadcast : ServerMsg -> unit


/// miscellaneous helper functions
[<AutoOpen>]
module Library = 

  open Newtonsoft.Json
  open System

  /// cleans up any disposable resource
  let inline dispose (o :^o :> IDisposable) = o.Dispose ()

  /// treats Choice2 constructs as either success or failure
  let inline (|Success|Failure|) value = 
    match value with
    | Choice1Of2 value -> Success value
    | Choice2Of2 value -> Failure value

  (* helpers to simplify working with JSON.NET *)

  let toJSON      value = JsonConvert.SerializeObject value
  let ofJSON<'T>  value = JsonConvert.DeserializeObject<'T> value 
  let tryJSON<'T> value = try
                            value |> ofJSON<ClientMsg> |> Choice1Of2
                          with 
                            | x ->  Choice2Of2 x.Message

  let encode = string >> System.Text.Encoding.UTF8.GetBytes
  let decode = System.Text.Encoding.UTF8.GetString

  (* extensions to DateTime for dealing with unix-epoch-derived values *)

  type System.DateTime with
    static member UnixEpoch = date(1970,1,1,0,0,0,DateTimeKind.Utc)
    static member inline SinceEpoch offset =
      (offset :^a when ^a : (static member op_Explicit : ^a -> float))
      |> float
      |> date.UnixEpoch.AddSeconds 
   
  (* operators and extensions for working with MailboxProcessors *)

  let inline (-->) (agent :agent<_>) msg = agent.PostAndReply msg
  let inline (<--) (agent :agent<_>) msg = agent.Post         msg
  
  type Microsoft.FSharp.Control.MailboxProcessor<'T> with
    member self.TryReceive (timeout :int<'u>) = self.TryReceive (int timeout)

  (* active recognizers to help with parsing *)

  /// safely decodes UTF8 data into a String
  let (|Str|_|) raw =
    try
      Some (decode raw)
    with
      | _ -> None
    
  /// safely converts raw data into a Dobule
  let (|Dbl|_|) raw =
    try
      (raw,0)
      |> BitConverter.ToDouble
      |> Some
    with
      | _ -> None

  /// safely converts raw data into a DateTime (via Unix epoch)
  let (|Dts|_|) raw =
    try
      (raw,0)
      |> BitConverter.ToInt64
      |> date.SinceEpoch
      |> Some
    with
      | _ -> None

  (* active recognizers to help convert from frames to objects *)

  /// validates framing and builds a Memo instance
  let (|Memo|_|) msg =
    match Array.length msg with
    | 1 ->  match msg.[0] with
            | Str frame ->  match frame |> String.split "\u221E" with
                            | [| sender; memo; |] -> Some { Sender  = sender
                                                            Message = memo }
                            | _                   -> None
            | _                                   -> None
    | _                                           -> None

  /// validates framing and builds a Tick instance
  let (|Tick|_|) msg =
    match Array.length msg with
    | 3 ->  match msg.[0],msg.[1],msg.[2] with
            | Str stock,Dts stamp,Dbl price -> Some { Stock = stock
                                                      Stamp = stamp
                                                      Price = price }
            | _                             -> None
    | _                                     -> None

  /// validates framing and builds a Calc instance
  let (|Calc|_|) msg =
    match Array.length msg with
    | 1 ->  match msg.[0] with
            | Dbl value -> Some { Value = value }
            | _         -> None
    | _                 -> None
