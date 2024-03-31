module Rules.MountWarn

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type SensitiveMount =
    {
      Code: string
      MountPoint: string
      Msg: string
    }
