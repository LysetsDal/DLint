module Rules.Mounts.Mnt124

open Rules.MountWarn

let mnt124 : SensitiveMount = {
    ErrorCode = "MNTW124"
    MountPoint = "/sys/firmware/efi/vars"
    ErrorMsg = "Exposes interfaces for interacting with EFI variables in NVRAM. Permission weaknesses here can rendered laptops unusable."
}