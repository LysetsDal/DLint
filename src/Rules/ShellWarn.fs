module Rules.ShellWarn

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type ShellWarn =
    {
      Code: string
      Bin: string
      Msg: string
    }