module Rules.Mounts.Mnt121

open Rules.MountWarn
//@TODO: Review
let mnt121 : SensitiveMount = {
    ErrorCode = "MNTW121"
    MountPoint = "/etc/passwd"
    ErrorMsg = "Mounting the hosts passwd file can leak sensitive user information to the container. Consider using Docker Secrets instead."
}