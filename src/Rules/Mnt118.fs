module Linterd.Rules.Mnt118

open Rules.MountWarn

let mnt118 : SensitiveMount = {
    Code = "Warning 118"
    MountPoint = "/proc/sys/kernel/modprobe"
    Msg = "Contains the path to the kernel module loader, invoked for loading
kernel modules (as root). An attacker can overwrite modprobe_path to point to
a malicious binary X."
}