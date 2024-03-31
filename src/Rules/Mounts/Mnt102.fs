module Rules.Mounts.Mnt102

open Rules.MountWarn

let mnt102 : SensitiveMount = {
    Code = "Warning 102"
    MountPoint = "/dev/kmem"
    Msg = "If /dev is visible privileges are too high. Risk exposing host syst-
em."
}