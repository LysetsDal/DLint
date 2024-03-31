module Linterd.Rules.Mounts.Mnt123

open Rules.MountWarn

let mnt123 : SensitiveMount = {
    Code = "Warning 123"
    MountPoint = "/sys/firmware/efi/efivars"
    Msg = "provides an interface to write to the NVRAM used for UEFI boot argu-
ments. Modifying them can render the host machine unbootable."
}