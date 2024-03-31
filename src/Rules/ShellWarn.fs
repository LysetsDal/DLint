module Linterd.Rules.ShellWarn

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type Binaries =
    {
      Code: string
      Bin: string
      Msg: string
    }