module Rules.Mounts.Mnt113

open Rules.MountWarn

let mnt113 : SensitiveMount = {
    ErrorCode = "MNTW113"
    MountPoint = "/proc/mem"
    ErrorMsg = "Alternate interface for /dev/mem, representing physical memory. Allows reading and writing, modification of all memory requires resolving virtual to physical addresses."
}