module Linterd.Rules.Mounts.Mnt112

open Rules.MountWarn

let mnt112 : SensitiveMount = {
    Code = "Warning 112"
    MountPoint = "/proc/kmsg"
    Msg = "Exposes kernel ring buffer messages. Can aid in kernel exploits, ad-
dress leaks, and provide sensitive system information."
}