module Linterd.Rules.Bash.SHB108

open Rules.ShellWarn

let shb108 : ShellWarn = {
    Code = "SHB108"
    Bin = "sudo"
    Msg = "Running sudo inside a Docker container is unnecessary and risky. Co-
ntainers should run with minimal privileges, and tasks requiring elevated perm-
issions should be managed outside the container or through Dockerfile configur-
ations."
}