module Rules.Bash.SHB101

open Rules.ShellWarn

let shb101 : ShellWarn = {
    Code = "SHB101"
    Bin = "sudo"
    Msg = "Running sudo inside containers can lead to unexpected behaviour and
higher privileges than are needed. Remove sudo from the command."
}
    