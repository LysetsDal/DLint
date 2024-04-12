module Rules.Mounts.Mnt105

open Rules.MountWarn

let mnt105 : SensitiveMount = {
    ErrorCode = "MNTW105"
    MountPoint = "/dev/sda1"
    ErrorMsg = "If /dev is visible privileges might be too high. Risk exposing host system."
}