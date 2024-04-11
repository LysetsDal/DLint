module Rules.Mounts.Mnt109

open Rules.MountWarn

let mnt109 : SensitiveMount = {
    ErrorCode = "MNTW109"
    MountPoint = "/proc/kallsyms"
    ErrorMsg = "This holds the kernel exported symbol definitions used by the modules() tools to dynamically link and bind loadable modules. Essential for kernel exploit development. Avoid mounting this into a container."
}