module Rules.Mounts.Mnt104

open Rules.MountWarn

let mnt104 : SensitiveMount = {
    ErrorCode = "MNTW104"
    MountPoint = "/dev/sda"
    ErrorMsg = "Careful, mounting this can grant access to the hosts main disk. Reconsider using devices."
}