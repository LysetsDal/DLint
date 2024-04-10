module Rules.Mounts.Mnt125

open Rules.MountWarn

let mnt125 : SensitiveMount = {
    ErrorCode = "MNTW125"
    MountPoint = "/sys/kernel/debug"
    ErrorMsg = "debugfs offers a 'no rules' debugging interface to the kernel (or k-
ernel modules). History of security issues due to its unrestricted nature."
}