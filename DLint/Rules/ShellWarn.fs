module Rules.ShellWarn

open System.Runtime.CompilerServices


[<IsReadOnly; Struct>]
type BinWarn =
    {
        ErrorCode: string
        Binary: string
        ErrorMsg: string
    }

[<IsReadOnly; Struct>]
type AptWarn =
    {
        ErrorCode: string
        Problem: string
        ErrorMsg: string
    }