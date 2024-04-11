module Rules.ShellWarn

open System.Runtime.CompilerServices


[<IsReadOnly; Struct>]
type binWarn =
    {
        ErrorCode: string
        Binary: string
        ErrorMsg: string
    }

[<IsReadOnly; Struct>]
type aptWarn =
    {
        ErrorCode: string
        Problem: string
        ErrorMsg: string
    }