module Rules.Bash.SHB101

open Rules.ShellWarn

let shb101 : BinWarn = {
    ErrorCode = "SHB101"
    Binary = "sudo"
    ErrorMsg = "Running sudo in the dockerfiles commands is unnecessary and risky. Containers should run with minimal privileges, and tasks requiring elevated permissions should be managed outside the container or through Dockerfile configurations."
}
    