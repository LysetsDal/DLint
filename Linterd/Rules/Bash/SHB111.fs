module Rules.Bash.SHB111

open Rules.ShellWarn

let shb111 : BinWarn = {
    ErrorCode = "SHB111"
    Binary = "service"
    ErrorMsg = "Using the service command inside a container may not work as expected, as it relies on the host system's init system, which may not be 'visible' or compatible within the container."
}