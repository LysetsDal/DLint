module Linterd.Rules.Mounts.Mnt115

open Rules.MountWarn

let mnt115 : SensitiveMount = {
    Code = "Warning 115"
    MountPoint = "/proc/sys/fs"
    Msg = "This directory /proc/sys/fs contains the files and subdirectories f-
or kernel variables related to filesystems. These variables can be read and in
some cases modified using the /proc filesystem."
}