module Rules.Mounts.Mnt115

open Rules.MountWarn

let mnt115 : SensitiveMount = {
    ErrorCode = "MNTW115"
    MountPoint = "/proc/sys/fs"
    ErrorMsg = "This directory /proc/sys/fs contains the files and subdirectories f-
or kernel variables related to filesystems. These variables can be read and in
some cases modified using the /proc filesystem."
}