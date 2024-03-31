module Linterd.Rules.Mounts.Mnt103

open Rules.MountWarn

let mnt103 : SensitiveMount = {
    Code = "Warning 103"
    MountPoint = "/dev/mem"
    Msg = "If /dev is visible privileges might be too high. Risk exposing host
system."
}