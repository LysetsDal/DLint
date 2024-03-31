module Linterd.Rules.Mounts.Mnt111

open Rules.MountWarn

let mnt111 : SensitiveMount = {
    Code = "Warning 111"
    MountPoint = "/proc/kmem"
    Msg = "Alternate interface for /dev/mem, representing physical memory. All-
ows reading and writing, modification of all memory requires resolving virtual
to physical addresses."
}