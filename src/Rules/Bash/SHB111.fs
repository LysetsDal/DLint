module Linterd.Rules.Bash.SHB111

open Rules.ShellWarn

let shb111 : binWarn = {
    Code = "SHB111"
    Bin = "service"
    Msg = "Using the service command inside a container may not work as expect-
ed, as it relies on the host system's init system, which may not be present or
compatible within the container."
}