module Linterd.Rules.Mnt127

open Rules.MountWarn

let mnt127 : SensitiveMount = {
    Code = "Warning 127"
    MountPoint = "/sys/kernel/uevent_helper"
    Msg = "The path for the uevent_helper can be modified by writing to this mo-
unt. Next time a uevent is triggered (which can also be done by writing to file-
s like /sys/class/mem/null/uevent), the malicious uevent_helper gets executed."
}