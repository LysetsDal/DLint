module Linterd.Rules.Mounts.Mnt114

open Rules.MountWarn

let mnt114 : SensitiveMount = {
    Code = "Warning 114"
    MountPoint = "/proc/sched_debug"
    Msg = "Returns process scheduling information, bypassing PID namespace pro-
tections. Can exposes process names, IDs, and cgroup identifiers."
}