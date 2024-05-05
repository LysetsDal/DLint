module Rules.Bash.SHB108

open Rules.ShellWarn

let shb108 : BinWarn = {
    ErrorCode = "SHB108"
    Binary = "cd"
    ErrorMsg = "You should use WORKDIR instead of cluttering run instructions with 'cd' commands, which are hard to read, troubleshoot, and maintain."
}