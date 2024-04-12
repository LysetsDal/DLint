module Rules.Mounts.Mnt124

open Rules.MountWarn

let mnt124 : SensitiveMount = {
    ErrorCode = "MNTW124"
    MountPoint = "/sys/firmware/efi/vars"
    ErrorMsg = "exposes interfaces for interacting with EFI variables in NVRAM. While this is not typically relevant for most servers, EFI is becoming more popular. Permission weaknesses here can rendered laptops unusable."
}