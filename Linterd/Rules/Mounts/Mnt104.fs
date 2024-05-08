module Rules.Mounts.Mnt104

open Rules.MountWarn

let mnt104 : SensitiveMount = {
    ErrorCode = "MNTW104"
    MountPoint = "/dev/sda"
    ErrorMsg = "If this is visible in /dev, privileges might be too high. Reconsider using devices."
}