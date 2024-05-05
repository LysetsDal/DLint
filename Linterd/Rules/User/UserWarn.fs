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
    ErrorMsg = "If a service can run without privileges, it is recommended to create a non-root user to run the container."
}