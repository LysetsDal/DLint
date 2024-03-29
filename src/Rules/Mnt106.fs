module Rules.Mnt106

open Rules.MountWarn

let mnt106 : SensitiveMount = {
    Code = "Warning 106"
    MountPoint = "/docker.sock"
    Msg = "Binding the docker.sock inside a container is not advised! A malici-
ous agent can spawn custom containers on the docker host machine with root pri-
vileges."
}