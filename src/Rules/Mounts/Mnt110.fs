module Rules.Mounts.Mnt110

open Rules.MountWarn

let mnt110 : SensitiveMount = {
    ErrorCode = "MNTW110"
    MountPoint = "/proc/kcore"
    ErrorMsg = "Represents the system's physical memory in ELF core format. Reading
can leak host system and other containers' memory contents. Large file size can
lead to reading issues or software crashes."
}