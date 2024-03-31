module Linterd.Rules.Mounts.Mnt108

open Rules.MountWarn

let mnt108 : SensitiveMount = {
    Code = "Warning 108"
    MountPoint = "/proc/config.gz"
    Msg = "May reveal the kernel configuration if CONFIG_IKCONFIG_PROC is enab-
led. Useful for attackers to identify vulnerabilities in the running kernel."
}