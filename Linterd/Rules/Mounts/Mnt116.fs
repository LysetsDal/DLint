module Rules.Mounts.Mnt116

open Rules.MountWarn

let mnt116 : SensitiveMount = {
    ErrorCode = "MNTW116"
    MountPoint = "/proc/sys/fs/binfmt_misc"
    ErrorMsg = "Allows registering interpreters for non-native binary formats based on their magic number. Can lead to privilege escalation or root shell access if /proc/sys/fs/binfmt_misc/register is writable."
}