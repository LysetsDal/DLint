module Rules.Mounts.Mnt111

open Rules.MountWarn

let mnt111 : SensitiveMount = {
    ErrorCode = "MNTW111"
    MountPoint = "/proc/kmem"
    ErrorMsg = "Alternate interface for /dev/mem, representing physical memory. Allows reading and writing, modification of all memory requires resolving virtual to physical addresses."
}