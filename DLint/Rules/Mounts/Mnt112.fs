module Rules.Mounts.Mnt112

open Rules.MountWarn

let mnt112 : SensitiveMount = {
    ErrorCode = "MNTW112"
    MountPoint = "/proc/kmsg"
    ErrorMsg = "Exposes kernel ring buffer messages. Can aid in kernel exploits, address leaks, and provide sensitive system information."
}