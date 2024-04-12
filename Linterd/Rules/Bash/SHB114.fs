module Rules.Bash.SHB114

open Rules.ShellWarn

let shb114 : BinWarn = {
    ErrorCode = "SHB114"
    Binary = "cd"
    ErrorMsg = "You should use WORKDIR instead of cluttering run instructions with 'cd' commands, which are hard to read, troubleshoot, and maintain."
}