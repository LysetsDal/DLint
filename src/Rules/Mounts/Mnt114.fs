module Rules.Mounts.Mnt114

open Rules.MountWarn

let mnt114 : SensitiveMount = {
    ErrorCode = "MNTW114"
    MountPoint = "/proc/sched_debug"
    ErrorMsg = "Returns process scheduling information, bypassing PID namespace pro-
tections. Can exposes process names, IDs, and cgroup identifiers."
}