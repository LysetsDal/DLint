module Rules.Mounts.Mnt103

open Rules.MountWarn

let mnt103 : SensitiveMount = {
    ErrorCode = "MNTW103"
    MountPoint = "/dev/mem"
    ErrorMsg = "If /dev is visible privileges might be too high. Risk exposing host system."
}