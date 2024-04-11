module Rules.Bash.SHB102

open Rules.ShellWarn

let shb102 : BinWarn = {
    ErrorCode = "SHB102"
    Binary = "shutdown"
    ErrorMsg = "Running shutdown inside a containers is nonsensical."
}
    