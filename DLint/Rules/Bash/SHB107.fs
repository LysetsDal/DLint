module Rules.Bash.SHB107

open Rules.ShellWarn

let shb107 : BinWarn = {
    ErrorCode = "SHB107"
    Binary = "su"
    ErrorMsg = "Using 'su' to switch user identity is discouraged within containers. User switching should be explicitly defined in the Dockerfile to maintain proper permissions."
}