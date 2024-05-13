module Rules.Mounts.Mnt128

open Rules.MountWarn

let mnt128 : SensitiveMount = {
    ErrorCode = "MNTW128"
    MountPoint = "/sys/kernel/vmcoreinfo"
    ErrorMsg = "This can leak kernel addresses which could be used to defeat KASLR. (Kernel Adress Space Layout Randomization). Loading the kernel to a random location can protect against attacks that rely on knowledge of kernel addresses."
}