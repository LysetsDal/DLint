module Rules.Mounts.Mnt129

open Rules.MountWarn

let mnt129 : SensitiveMount = {
    ErrorCode = "MNTW129"
    MountPoint = "/etc/shadow"
    ErrorMsg = "You are mounting the password hash file into the container. Wh-
ile not inherently dangerous, it can make dictionary attacks eaisier for adver-
saries."
}