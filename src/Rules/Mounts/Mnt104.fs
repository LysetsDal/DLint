module Rules.Mounts.Mnt104

open Rules.MountWarn

let mnt104 : SensitiveMount = {
    Code = "Warning 104"
    MountPoint = "/dev/sda"
    Msg = "If /dev is visible privileges might be too high. Risk exposing host
system."
}