module Rules.Misc.NetWarn100

open Rules.MiscWarn

let netWarn100 : MiscWarn = {
    ErrorCode = "NETW100"
    Problem =  "--network=host"
    ErrorMsg = "RUN --network allows control over which networking environment the command is run in. With this setting the container is run in the host's network environment. The use of --network=host should be carefully considered, as it can leak the host machines network interface to the container."
}
