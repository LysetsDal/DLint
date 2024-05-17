module Rules.Mounts.Mnt106

open Rules.MountWarn

let mnt106 : SensitiveMount = {
    ErrorCode = "MNTW106"
    MountPoint = "/docker.sock"
    ErrorMsg = "Binding the docker.sock inside a container is strongly advised against! A malicious agent can create containers on the docker host, and escalate privileges."
}