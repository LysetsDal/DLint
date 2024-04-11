module Rules.Mounts.Mnt108

open Rules.MountWarn

let mnt108 : SensitiveMount = {
    ErrorCode = "MNTW108"
    MountPoint = "/proc/config.gz"
    ErrorMsg = "May reveal the kernel configuration if CONFIG_IKCONFIG_PROC is enabled. Useful for attackers to identify vulnerabilities in the running kernel."
}