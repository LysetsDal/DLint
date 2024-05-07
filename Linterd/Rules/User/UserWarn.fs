module Rules.Misc.UserWarns

open Rules.MiscWarn

let userWarn100 : MiscWarn = {
    ErrorCode = "USERW100"
    Problem = "Warning: running as root!"
    ErrorMsg = "This container is running as root. In event of a container breakout, the adversary will have the host machines root privileges."
}


let userWarn101 : MiscWarn = {
    ErrorCode = "USERW101"
    Problem = "Warning: No user specified!"
    ErrorMsg = "Configuring the container to use an unprivileged user is the best way to prevent privilege escalation attacks. Add a USER instruction to the Dockerfile"
}