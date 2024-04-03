module Rules.Bash.SHB101

open Rules.ShellWarn

let shb101 : binWarn = {
    Code = "SHB101"
    Bin = "sudo"
    Msg = "Running sudo inside a Docker container is unnecessary and risky. Co-
ntainers should run with minimal privileges, and tasks requiring elevated perm-
issions should be managed outside the container or through Dockerfile configur-
ations."
}
    