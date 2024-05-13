module Rules.Mounts.Mnt121

open Rules.MountWarn

let mnt121 : SensitiveMount = {
    ErrorCode = "MNTW121"
    MountPoint = "/etc/passwd"
    ErrorMsg = "CAUTION: Mounting the hosts passwd file can leak sensitive user information to the container. Consider using Docker Secrets instead."
}