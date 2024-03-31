module Linterd.Rules.Bash.SHB107

open Rules.ShellWarn

let shb107 : ShellWarn = {
    Code = "SHB107"
    Bin = "su"
    Msg = "Using 'su' to switch user identity is discouraged within containers.
User switching should be explicitly defined in the Dockerfile to maintain prop-
er permissions."
}