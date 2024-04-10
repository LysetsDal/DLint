module Rules.Mounts.Mnt102

open Rules.MountWarn

let mnt102 : SensitiveMount = {
    ErrorCode = "MNTW102"
    MountPoint = "/dev/kmem"
    ErrorMsg = "If /dev is visible privileges are too high. Risk exposing host syst-
em."
}