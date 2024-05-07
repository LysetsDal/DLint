module Rules.Mounts.Mnt103

open Rules.MountWarn

let mnt103 : SensitiveMount = {
    ErrorCode = "MNTW103"
    MountPoint = "/dev/mem"
    ErrorMsg = "If this is visible in /dev, privileges might be too high. Reconsider using devices."
}