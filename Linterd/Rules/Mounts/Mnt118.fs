module Rules.Mounts.Mnt118

open Rules.MountWarn

let mnt118 : SensitiveMount = {
    ErrorCode = "MNTW118"
    MountPoint = "/proc/sys/kernel/modprobe"
    ErrorMsg = "Contains the path to the kernel module loader, invoked for loading
kernel modules (as root). An attacker can overwrite modprobe_path to point to
a malicious binary X."
}