module Rules.Mounts.Mnt123

open Rules.MountWarn

let mnt123 : SensitiveMount = {
    ErrorCode = "MNTW123"
    MountPoint = "/sys/firmware/efi/efivars"
    ErrorMsg = "provides an interface to write to the NVRAM used for UEFI boot argu-
ments. Modifying them can render the host machine unbootable."
}