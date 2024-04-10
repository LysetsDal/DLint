module Rules.MiscWarn

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type MiscWarn =
    {
      ErrorCode: string
      Problem: string
      ErrorMsg: string
    }
