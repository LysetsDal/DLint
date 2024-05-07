module Rules.Mounts.Mnt101

open Rules.MountWarn

let mnt101 : SensitiveMount = {
    ErrorCode = "MNTW101"
    MountPoint = "/dev/disk"
    ErrorMsg = "If this is visible in /dev, privileges might be too high. Reconsider using devices."
}
