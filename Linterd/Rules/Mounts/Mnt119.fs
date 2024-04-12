module Rules.Mounts.Mnt119

open Rules.MountWarn
//@TODO: Needs sources...
let mnt119 : SensitiveMount = {
    ErrorCode = "MNTW119"
    MountPoint = "/proc/sys/vm/panic_on_oom"
    ErrorMsg = "A global flag that controls whether the kernel panics or invokes the OOM-killer when an Out Of Memory condition occurs. Can be used to manipulate and stall vm's by disrupting the standard reaction on OOM errors."
}