module Rules.Mnt101

open Rules.MountWarn

let mnt101 : SensitiveMount = {
    Code = "Warning 101"
    MountPoint = "/dev/disk"
    Msg = "If /dev is visible privileges might be too high. Risk exposing host
system."
}
