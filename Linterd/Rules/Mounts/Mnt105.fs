module Rules.Mounts.Mnt105

open Rules.MountWarn

let mnt105 : SensitiveMount = {
    ErrorCode = "MNTW105"
    MountPoint = "/dev/sdb"
    ErrorMsg = "If this is visible in /dev, privileges might be too high. Reconsider using devices."
}