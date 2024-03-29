module Linterd.Rules.Mnt113

open Rules.MountWarn

let mnt113 : SensitiveMount = {
    Code = "Warning 113"
    MountPoint = "/proc/mem"
    Msg = "Alternate interface for /dev/mem, representing physical memory. All-
ows reading and writing, modification of all memory requires resolving virtual
to physical addresses."
}