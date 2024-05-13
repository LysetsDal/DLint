module Rules.Mounts.Mnt129

open Rules.MountWarn

let mnt129 : SensitiveMount = {
    ErrorCode = "MNTW129"
    MountPoint = "/etc/shadow"
    ErrorMsg = "CAUTION: You are mounting the passwords hash file into the container! It can leak passwords and make dictionary attacks easier."
}