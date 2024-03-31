module Rules.Mounts.Mnt124

open Rules.MountWarn

let mnt124 : SensitiveMount = {
    Code = "Warning 124"
    MountPoint = "/sys/firmware/efi/vars"
    Msg = "exposes interfaces for interacting with EFI variables in NVRAM. Whi-
le this is not typically relevant for most servers, EFI is becoming more popul-
ar. Permission weaknesses here can rendered laptops unusable."
}