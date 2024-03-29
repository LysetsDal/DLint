module Linterd.Rules.Mnt119

open Rules.MountWarn
// Needs sources...
let mnt119 : SensitiveMount = {
    Code = "Warning 119"
    MountPoint = "/proc/sys/vm/panic_on_oom"
    Msg = "A global flag that controls whether the kernel panics or invokes the
OOM-killer when an Out Of Memory condition occurs. Can be used to manipulate a-
nd stall vm's by disrupting the standard reaction on OOM errors."
}