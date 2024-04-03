module Rules.Bash.SHB106

open Rules.ShellWarn

let shb106 : binWarn = {
    Code = "SHB106"
    Bin = "ssh"
    Msg = "SSH logs into remote machines and runs commands. Running it in a co-
ntainer complicates configuration and adds security risks. Manage containers u-
sing Docker CLI, API, or Desktop App."
}