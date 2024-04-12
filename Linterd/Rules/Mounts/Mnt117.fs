module Rules.Mounts.Mnt117

open Rules.MountWarn

let mnt117 : SensitiveMount = {
    ErrorCode = "MNTW117"
    MountPoint = "/proc/sys/kernel/core_pattern"
    ErrorMsg = "The /proc/sys/kernel/core_pattern file can be set to define a template that is used to name core dump files. This can lead to code execution if the file begins with a pipe '|' character."
}