module Rules.Mounts.Mnt102

open Rules.MountWarn

let mnt102 : SensitiveMount = {
    ErrorCode = "MNTW102"
    MountPoint = "/dev/kmem"
    ErrorMsg = "If this is visible in /dev, privileges might be too high. Reconsider using devices."
}