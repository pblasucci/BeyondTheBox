open System
open System.Diagnostics
open System.IO
open System.Threading

// path to Windows
let winroot = "/Volumes/C/Developer/BeyondTheBox/Source/"

// path to tickz publisher
let tickz = Path.Combine (winroot,"tickz/tickz.server/Debug/tickz.server.exe")
// path to chatz server
let chatz = Path.Combine (winroot,"chatz/chatz.server/chatz.server.exe")

// path to dealz client
let dealz = Path.Combine (__SOURCE_DIRECTORY__,"../dealz/dealz.host/bin/Debug/dealz.host.exe")
// path to valuz worker
let valuz = Path.Combine (__SOURCE_DIRECTORY__,"../valuz/valuz.worker/valuz.worker")

// launch tickz
Process.Start tickz |> ignore
// launch chatz
Process.Start chatz |> ignore
// launch dealz host
Process.Start ("bash",sprintf "-c 'mono %s pblasucci %s %i'" dealz valuz 3) |> ignore
// launch dealz gui
Process.Start "http://localhost:9000/dealz.html" |> ignore
