module Rules.Mounts.Mnt107

open Rules.MountWarn

let mnt107 : SensitiveMount = {
    Code = "Warning 107"
    MountPoint = "/proc/[0-9]/mem"
    Msg = "Interfaces with the kernel memory device /dev/mem. Historically vul-
nerable to privilege escalation attacks.This file can be used to access the pa-
ges of a process's memory through open(2), read(2), and lseek(2)."
}