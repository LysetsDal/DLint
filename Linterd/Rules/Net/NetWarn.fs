module Rules.Misc.NetWarn100

open Rules.MiscWarn

let netWarn100 : MiscWarn = {
    ErrorCode = "NETW100"
    Problem =  "--network=host"
    ErrorMsg = "The --network=host option links the container to the host's network interface, providing it with network access equivalent to host processes. Use with care!"
}
