module Rules.MountWarn

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type SensitiveMount =
    {
      ErrorCode: string
      MountPoint: string
      ErrorMsg: string
    }
