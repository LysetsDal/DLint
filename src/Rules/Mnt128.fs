module Linterd.Rules.Mnt128

open Rules.MountWarn

let mnt128 : SensitiveMount = {
    Code = "Warning 128"
    MountPoint = "/sys/kernel/vmcoreinfo"
    Msg = "This can leak kernel addresses which could be used to defeat KASLR.
(Kernel Adress Space Layout Randomization). Loading the kernel to a random loca-
tion can protect against attacks that rely on knowledge of kernel addresses."
}