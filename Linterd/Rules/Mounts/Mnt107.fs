module Rules.Mounts.Mnt107

open Rules.MountWarn

let mnt107 : SensitiveMount = {
    ErrorCode = "MNTW107"
    MountPoint = "/proc/[0-9]/mem"
    ErrorMsg = "Interfaces with the kernel memory device /dev/mem. Historically vulnerable to privilege escalation attacks.This file can be used to access the pages of a process's memory through open(2), read(2), and lseek(2)."
}