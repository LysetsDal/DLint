module Rules.Mounts.Mnt126

open Rules.MountWarn

let mnt126 : SensitiveMount = {
    ErrorCode = "MNTW126"
    MountPoint = "/sys/kernel/security"
    ErrorMsg = "Here the securityfs interface is mounted, which allows configuration
of Linux Security Modules. This allows configuration of AppArmor policies, and
access to this mount may allow a container to disable its MAC system."
}