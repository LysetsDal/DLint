module Rules.Mounts.Mnt126

open Rules.MountWarn

let mnt126 : SensitiveMount = {
    ErrorCode = "MNTW126"
    MountPoint = "/sys/kernel/security"
    ErrorMsg = "Here resides the Linux Security Modules, which allows configuration of SELinux and AppArmor policies. Access to this mount may allow a container to replace policies and disable Mandatory Access Control."
}