module Rules.MountWarn

type SensitiveMount =
    { Code: string
      MountPoint: string
      Msg: string
    }
