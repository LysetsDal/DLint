module Rules.ShellWarn

open System.Runtime.CompilerServices


[<IsReadOnly; Struct>]
type binWarn =
    {
        Code: string
        Bin: string
        Msg: string
    }

[<IsReadOnly; Struct>]
type aptWarn =
    {
        Code: string
        Problem: string
        Msg: string
    }