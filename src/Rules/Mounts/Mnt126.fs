module Rules.Mounts.Mnt126

open Rules.MountWarn

let mnt126 : SensitiveMount = {
    Code = "Warning 126"
    MountPoint = "/sys/kernel/security"
    Msg = "Here the securityfs interface is mounted, which allows configuration
of Linux Security Modules. This allows configuration of AppArmor policies, and
access to this mount may allow a container to disable its MAC system."
}