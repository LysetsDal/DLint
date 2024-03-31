module Rules.Mounts.Mnt120

open Rules.MountWarn

let mnt120 : SensitiveMount = {
    Code = "Warning 120"
    MountPoint = "/proc/sysrq-trigger"
    Msg = "This file controls the functions allowed to be invoked by the SysRq
key. Possible SysRq commands include setting kernel logging-levels, signaling
processes, and causing immediate shutdowns & reboots."
}