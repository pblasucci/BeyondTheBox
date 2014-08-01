open System
open System.Diagnostics
open System.IO
open System.Threading


do (* PROGRAM *)
  // path to tickz publisher
  let tickz = Path.Combine (__SOURCE_DIRECTORY__
                           ,"../tickz/tickz.server/Debug/tickz.server.exe")
  // path to chatz server
  let chatz = Path.Combine (__SOURCE_DIRECTORY__
                           ,"../chatz/chatz.server/chatz.server.exe")
  // path to dealz client
  let dealz = Path.Combine (__SOURCE_DIRECTORY__
                           ,"dealz/bin/Debug/dealz.exe")

  // program configuration
  let debug   = match fsi.CommandLineArgs with
                | [| _; "debug" |]  -> true
                | _                 -> false

  // launch tickz
  Process.Start tickz |> ignore
  // launch chatz
  if debug then Environment.SetEnvironmentVariable("RUST_LOG","chatz.server=4")
  Process.Start chatz |> ignore
  // launch dealz
  Process.Start dealz |> ignore
  Process.Start "http://localhost:9000/dealz.html" |> ignore
