module Rules.Mounts.Mnt101

open Rules.MountWarn

let mnt101 : SensitiveMount = {
    ErrorCode = "MNTW101"
    MountPoint = "/dev/disk"
    ErrorMsg = "If /dev is visible privileges might be too high. Risk exposing host system."
}
