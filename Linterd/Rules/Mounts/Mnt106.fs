module Rules.Mounts.Mnt106

open Rules.MountWarn

let mnt106 : SensitiveMount = {
    ErrorCode = "MNTW106"
    MountPoint = "/docker.sock"
    ErrorMsg = "Binding the docker.sock inside a container is not advised! A malicious agent can spawn custom containers on the docker host machine with root privileges."
}