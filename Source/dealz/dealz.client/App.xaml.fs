module dealz.client.program

open System
open FsXaml

type App = XAML<"App.xaml">

[<STAThread;EntryPoint>]
let main _ =
  App().CreateRoot().Run()
