module Rules.Mounts.Mnt105

open Rules.MountWarn

let mnt105 : SensitiveMount = {
    ErrorCode = "MNTW105"
    MountPoint = "/dev/sdb"
    ErrorMsg = "Careful, mounting this can grant access to the hosts second disk. Reconsider using devices."
}