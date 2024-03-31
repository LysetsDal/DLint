module Rules.Mounts.Mnt110

open Rules.MountWarn

let mnt110 : SensitiveMount = {
    Code = "Warning 110"
    MountPoint = "/proc/kcore"
    Msg = "Represents the system's physical memory in ELF core format. Reading
can leak host system and other containers' memory contents. Large file size can
lead to reading issues or software crashes."
}