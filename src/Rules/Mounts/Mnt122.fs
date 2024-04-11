module Rules.Mounts.Mnt122

open Rules.MountWarn

let mnt122 : SensitiveMount = {
    ErrorCode = "MNTW122"
    MountPoint = "/sys/class/thermal"
    ErrorMsg = "Access to ACPI and various hardware settings for temperature control,
typically found in laptops or gaming motherboards. This may allow for DoS attac-
ks against the container host, which may even lead to physical damage."
}