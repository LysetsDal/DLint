module Rules.Mounts.Mnt105

open Rules.MountWarn

let mnt105 : SensitiveMount = {
    Code = "Warning 105"
    MountPoint = "/dev/sda1"
    Msg = "If /dev is visible privileges might be too high. Risk exposing host
system."
}