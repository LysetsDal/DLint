module Rules.Mounts.Mnt109

open Rules.MountWarn

let mnt109 : SensitiveMount = {
    Code = "Warning 109"
    MountPoint = "/proc/kallsyms"
    Msg = "This holds the kernel exported symbol definitions used by the
modules() tools to dynamically link and bind loadable modules. Essential for k-
ernel exploit development. Avoid mounting this into a container."
}