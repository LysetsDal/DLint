module Rules.Bash.SHB106

open Rules.ShellWarn

let shb106 : BinWarn = {
    ErrorCode = "SHB106"
    Binary = "ssh"
    ErrorMsg = "SSH logs into remote machines and runs commands. Running it in a container complicates configuration and adds security risks. Manage containers using Docker CLI, API, or Desktop App."
}