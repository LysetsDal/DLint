module Rules.Mounts.Mnt104

open Rules.MountWarn

let mnt104 : SensitiveMount = {
    ErrorCode = "MNTW104"
    MountPoint = "/dev/sda"
    ErrorMsg = "If /dev is visible privileges might be too high. Risk exposing host system."
}